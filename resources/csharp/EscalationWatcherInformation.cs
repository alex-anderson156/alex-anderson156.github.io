namespace SquawkHQ.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Bedrock;
    using SquawkCore.DataAccess;
    using SquawkCore.EntityModels;
    using SquawkCore.Code.Calculators;
    using SquawkCore.Enums;
    using Models;
    using Code;
    using SquawkCore.Models;

    /// <summary>
    /// Contains information relevant to watching Data for an Escalation including;
    /// - The Escalation State of all Features
    /// </summary>
    public class EscalationWatcherInformation : WatcherInformation
    {
        // --
        // Fields and properties

        /// <summary>
        /// Gets or sets a List of All Squawks active on the machine
        /// (note: _New is only a temporary Suffix)
        /// </summary>
        public List<Squawk_New> Squawks { get; set; }

        /// <summary>
        /// Gets or sets a list of all Escalation States for each Squawk
        /// </summary>
        public Dictionary<Squawk_New, EscalationData> SquawkEscalationStates { get; set; }

        // --
        // Constructor


        public EscalationWatcherInformation()
        {
            SquawkEscalationStates = new Dictionary<Squawk_New, EscalationData>();
        }

        // --
        // Methods


        /// <summary>
        ///  Loads all relevant Information about Escalations at the Start of a new DataWatcher including:
        ///  - Squawks
        ///  - Reloading All Current Escalation Procedures (Dropped or Invalid)
        /// </summary>
        /// <param name="watcher">The Watcher Instance</param>
        /// <param name="db">an Open Database Connection</param>
        public override void Load(DataWatcher watcher, DCPDataDatabase db)
        {
            Watcher = watcher;
            Squawks = db.LoadSquawksFor(Asset, CurrentComponent.Component);
            SquawkEscalationStates = new Dictionary<Squawk_New, EscalationData>();

            List<EscalationLog> previousLogs = db.EscalationLogRepository.LoadLastEscalationFor(Squawks).ToList();

            foreach (Squawk_New squawk in Squawks)
            {
                EscalationLog previousEsc = previousLogs.SingleOrDefault(x => x.Squawk.Equals(squawk));
                EscalationData escData = new EscalationData(previousEsc);

                // Check if the Previous log was Finished (is the Last Entry a Reason )
                if (previousEsc != null && previousEsc.LatestLog != null)
                {
                    // Look at the Latest Log Entry -
                    // If it was a reason, then we need to do nothing, it was completed.
                    if(previousEsc.LatestLog.ProcedureNode.ReasonFID == null)
                    {
                        Feature feature = squawk.AssetFeatureTolerance.ProcessFeatureTolerance.ProcessFeature.Feature;

                        // Node was not a reason node,
                        //  We need to Resume this Escalation
                        escData.CurrentRunner = EscalationProcedureRunnerFactory.ResumeEscalation(this, squawk, previousEsc);
                        Watcher.NewsFeed.EscalationStates.Add(new EscalationStatus(squawk.SquawkUID, feature.FeatureName, escData.CurrentRunner.EscalationState, LastProductionTimestamp));

                        // *** No Need to Notify Subscribers
                        // *** They Will Pick this up in the News Feed Push!
                        //AllClients.escalationEventStart(new
                        //{
                        //    SquawkUID = squawk.SquawkUID,
                        //    Feature = squawk.AssetFeatureTolerance.ProcessFeatureTolerance.ProcessFeature.Feature.FeatureName,
                        //    EscalationState = escalationType,
                        //    EventStartTime = LastProductionTimestamp
                        //});

                        escData.CurrentRunner.Resume();
                    }
                }

                SquawkEscalationStates.Add(squawk, escData);
            }
        }

        /// <summary>
        /// Invoked when a new Data Signal is received from the Database
        /// </summary>
        /// <param name="db">An Open Connection to the Database</param>
        /// <param name="latestPart">The latest Part to have been processed</param>
        /// <param name="allData">a Set of All Data added since the last Sweep</param>
        public override void NewDataRecieved(DCPDataDatabase db, ProductionData latestPart, List<ProductionData> allData)
        {
            int maxFrameSize = Squawks.Max(s => s.FrameSize);

            Feature[] features = Squawks.Select(x => x.AssetFeatureTolerance.ProcessFeatureTolerance.ProcessFeature.Feature).ToList().ToArray();
            List<ProductionData> lastFrameData = db.LoadLastPartsForAsset(Asset, CurrentComponent.Component, maxFrameSize, features).OrderByDescending(x => x.DateTime).ToList();

            foreach (Squawk_New squawk in Squawks)
            {
                SquawkProcedureChangeLog latestChangelog = db.GetLatestProcedureChangelogFor(squawk);

                if(latestChangelog == null) {
                    // There is no Escalation Avalaible,
                    continue;
                }

                EscalationData currentEscalation = SquawkEscalationStates[squawk];

                Feature feature = squawk.AssetFeatureTolerance.ProcessFeatureTolerance.ProcessFeature.Feature;
                DateTime minCutoffTime = DateTime.Now - TimeSpan.FromSeconds((1.5 * CurrentComponent.CycleTime) * squawk.FrameSize);

                DateTime lastEscDate = SquawkEscalationStates[squawk].LastEscalation == null ?
                    minCutoffTime : (new DateTime[] { minCutoffTime, SquawkEscalationStates[squawk].LastEscalation.Entries.Max(x => x.LogTime) }.Max());

                lastFrameData = lastFrameData.Where(data => data.DateTime > lastEscDate).ToList();

                //
                if(lastFrameData.Count < squawk.FrameSize)
                {
                    SquawkHQLogger.Log(ConsoleColor.Cyan, "Too Few parts to determine Escalation for {1}. ({0})", lastFrameData.Count, feature.FeatureName);
                    continue;
                }

                lastFrameData.Map(x => {  x.MainFeature = x.FeatureValues.Single(y => y.Key.Equals(feature)); });

                using (EscalationWeightCalculator escCalculator = new EscalationWeightCalculator())
                {
                    escCalculator.AssetFeatureTolerance = squawk.AssetFeatureTolerance;
                    escCalculator.ProcessFeatureTolerance = squawk.AssetFeatureTolerance.ProcessFeatureTolerance;
                    escCalculator.Squawk = squawk;

                    Tuple<int, int> weights = escCalculator.Calculate(lastFrameData.Take(squawk.FrameSize).ToList());
                    int yellowWeight = weights.Item1;
                    int redWeight = weights.Item2;

                    Console.WriteLine("@@ {0:00000}-{1} Weightings Red:'{2:000}' Yellow:'{3:000}'".Args(Asset.AssetNumber, feature.FeatureName, redWeight, yellowWeight));


                    if(currentEscalation.IsInEscalation)
                    {
                        // We are currently in Escalation
                        // Get the Latest Log Entry
                        // If we are RED - Are we downgraded to Yellow?
                        // If we are Yellow - Are we downgraded to None?
                    }
                    else
                    {
                        if(redWeight >= squawk.RedLimit) {
                            // Go Into Red Escalation
                            OpenEscalationEvent(squawk, EscalationType.Red);
                        }
                        else if (yellowWeight >= squawk.YellowLimit) {
                            // Go into Yellow Escalaiton
                            OpenEscalationEvent(squawk, EscalationType.Yellow);
                        }
                        else {
                            // No Escalation
                        }
                    }

                }
            }

        }

        /// <summary>
        /// Invoked on part Change over
        /// (not Implemented yet)
        /// </summary>
        public override void OnChangeover()
        {

        }

        // --
        // IWatcherInformation Methods

        /// <summary>
        /// Invoked at the Start of a Shift
        /// </summary>
        /// <param name="currentShift">The Shift</param>
        /// <param name="currentTiming">The Working Timing Block (if any)</param>
        public override void StartShift(TimeFrame currentShift, ShiftTimings currentTiming)
        {

        }

        /// <summary>
        /// Invoked at the End of a Shift
        /// </summary>
        /// <param name="currentShift">The Shift</param>
        /// <param name="currentTiming">The Last priod (either last half hour or Last Hour)</param>
        public override void EndShift(TimeFrame currentShift, DateTime start, DateTime end)
        {

        }

        /// <summary>
        /// Invoked after a Break from Work (no Timing Peroid)
        /// </summary>
        /// <param name="currentTiming">The Last priod (either last half hour or Last Hour)</param>
        public override void OnReturnFromBreak(ShiftTimings currentTiming)
        {

        }

        /// <summary>
        /// Invoked when a Timing Event occurs (See NextSliceAction)
        /// </summary>
        /// <param name="action">The Action Being Performed, (Top of hour etc.) </param>
        /// <param name="start">The Start of the period</param>
        /// <param name="end">The End of the period></param>
        public override void CurrentTimingEvent(NextSliceAction action, DateTime start, DateTime end)
        {

        }

        // --
        //

        /// <summary>
        /// Opens a new Escalation Event against the squawk
        /// </summary>
        /// <param name="squawk">The Squawk to open against</param>
        /// <param name="escalationType">The Type of Escalation (Yellow / Red)</param>
        public void OpenEscalationEvent(Squawk_New squawk, EscalationType escalationType)
        {
            ConsoleColor color = escalationType == EscalationType.Yellow ? ConsoleColor.DarkYellow : ConsoleColor.DarkMagenta;
            string feature = squawk.AssetFeatureTolerance.ProcessFeatureTolerance.ProcessFeature.Feature.FeatureName;
            SquawkHQLogger.Log(color, "{0} Escalation Entered!".Args(EnumHelper.EnumToString(escalationType)));

            SquawkEscalationStates[squawk].CurrentRunner = EscalationProcedureRunnerFactory.StartNew(this, squawk, escalationType, LastProductionTimestamp);

            Watcher.NewsFeed.EscalationStates.Add(new EscalationStatus(squawk.SquawkUID, feature, escalationType, LastProductionTimestamp));
            AllClients.escalationEventStart(new
            {
                SquawkUID = squawk.SquawkUID,
                Feature = squawk.AssetFeatureTolerance.ProcessFeatureTolerance.ProcessFeature.Feature.FeatureName,
                EscalationState = escalationType,
                EventStartTime = LastProductionTimestamp
            });

            SquawkEscalationStates[squawk].CurrentRunner.Run();
        }

        /// <summary>
        /// Closes a new Escalation Event against the squawk
        /// </summary>
        /// <param name="squawk">The Squawk to close against</param>
        public void CloseEscalationEvent(Squawk_New squawk)
        {
            // Remove it from the News Feed
            EscalationStatus newsEntry = Watcher.NewsFeed.EscalationStates.Single(e => e.SquawkUID == squawk.SquawkUID);
            Watcher.NewsFeed.EscalationStates.Remove(newsEntry);

            // Notify Subscribers
            AllClients.escalationEventEnd(new { SquawkUID = squawk.SquawkUID });
            SquawkEscalationStates[squawk].LastEscalation = SquawkEscalationStates[squawk].CurrentRunner.Log;
            SquawkEscalationStates[squawk].CurrentRunner = null;
        }

        /// <summary>
        /// Occurs when an Escalation Changes State (Yello to Red or Vsv)
        /// </summary>
        /// <param name="squawk">The Squawk to Modify</param>
        /// <param name="newState">The New State of the Squawk</param>
        public void OnEscalationStateChanged(Squawk_New squawk, EscalationType newState)
        {
            EscalationStatus newsEntry = Watcher.NewsFeed.EscalationStates.Single(e => e.SquawkUID == squawk.SquawkUID);
            newsEntry.State = newState;

            AllClients.escalationEventChanged(new {
                SquawkUID = squawk.SquawkUID,
                EscalationState = newState,
            });
        }

        /// <summary>
        /// Occurs when a client subscriber has posted a result to the corrective action
        /// </summary>
        /// <param name="assetNumber">The Asset unmber involved</param>
        /// <param name="actionChangelogFID">The Current Corrective Action Changelog</param>
        /// <param name="squawkUID">The Squawk</param>
        /// <param name="actionResult">The Result Selected</param>
        /// <param name="input">any Input from the operator (if any)</param>
        /// <returns>true - can submit correctly, false - somone has already submitted this.</returns>
        public bool PostCorrectiveActionResult(int assetNumber, int actionChangelogFID, int squawkUID, string actionResult, string input)
        {
            Squawk_New squawk = Squawks.Single(s => s.SquawkUID == squawkUID);
            EscalationProcedureRunner runner = SquawkEscalationStates[squawk].CurrentRunner;

            runner?.OnCorrectiveActionResult(actionChangelogFID, actionResult, input);
            return true;
        }
    }
}
