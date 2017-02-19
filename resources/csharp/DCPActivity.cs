namespace AppCore
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AppCore;
    using AppCore.ChartCore;
    using AppCore.Components;
    using AppCore.TileCore;
    using AppCore.Tiles;
    using System.Threading;
    using AppCore.DataGridCore;
    using AppCore.Enums;
    using System.Data.SqlClient;
    using AppCore.CanvasCore;
using AppCore.Palette;

    public enum DCPSTATE
    {
        Created = 1,
        Loaded = 2,
        Paused = 4,
    }

    public static class DCPStateExtentions
    {
        public static bool IsCreated(this DCPSTATE state)
        {
            return (state & DCPSTATE.Created) == DCPSTATE.Created;
        }

        public static bool IsLoaded(this DCPSTATE state)
        {
            return (state & DCPSTATE.Loaded) == DCPSTATE.Loaded;
        }

        public static bool IsPaused(this DCPSTATE state)
        {
            return (state & DCPSTATE.Paused) == DCPSTATE.Paused;
        }
    }

    public abstract class DCPActivity : ViewModelBase
    {
        private DCPSTATE ActivityState
        {
            get
            {
                return activityState;
            }
            set
            {
                StateCheck(activityState, value);
                activityState = value;
            }
        }
        private DCPSTATE activityState = 0;

        private void SetState(DCPSTATE state)
        {
            ActivityState = ActivityState | state;
        }

        private void UnsetState(DCPSTATE state)
        {
            ActivityState = ActivityState ^ state;
        }

        private void StateCheck(DCPSTATE oldState, DCPSTATE newState)
        {
           // If the current State is Loaded and Created.
            if (newState.IsLoaded() && newState.IsCreated())
            {
                DCPSTATE state = oldState ^ newState;

                if (state.IsLoaded() || state.IsCreated())
                    OnCreateComplete();
            }

        }

        // --
        // Properties & Fields

        /// <summary>
        /// Gets or sets the Application Host
        /// </summary>
        public DCPAppBase AppHost { get; private set; }

        /// <summary>
        /// Gets the Activities View
        /// </summary>
        public UserControl View { get; private set; }

        /// <summary>
        /// Gets or sets the TileSet
        /// </summary>
        public SetBuilder TileSet
        {
            get { return tileSet; }
            set
            {
                tileSet = value;
                RaisePropertyChanged("TileSet");
            }
        }
        private SetBuilder tileSet;

        /// <summary>
        /// Gets or sets the ActionBar
        /// </summary>
        public FixedActionBar ActionBar
        {
            get { return actionBar; }
            set
            {
                actionBar = value;
                RaisePropertyChanged("ActionBar");
            }
        }
        private FixedActionBar actionBar;

        /// <summary>
        /// Gets or sets a falg indicating wether the Activity is Currently Killable.
        /// </summary>
        public bool IsKillable { get; set; }

        /// <summary>
        /// Gets or sets the IsWorking
        /// </summary>
        public bool IsWorking
        {
            get { return AppHost.IsWorking; }
            set
            {
                if(AppHost != null)
                    AppHost.IsWorking = value;

                RaisePropertyChanged("IsWorking");
            }
        }

        /// <summary>
        /// Gets or sets the LoadingInformation
        /// </summary>
        public string LoadingInformation
        {
            get { return string.IsNullOrEmpty(activityLoadingInformation) ? activityLoadingInformation : AppHost.LoadingInformation; }
            set
            {
                activityLoadingInformation = value;
                RaisePropertyChanged("LoadingInformation");
            }
        }
        private string activityLoadingInformation = string.Empty;

        /// <summary>
        /// Gets or sets the ActivityHeader
        /// </summary>
        public string ActivityHeader
        {
            get { return activityHeader; }
            set
            {
                activityHeader = value;
                RaisePropertyChanged("ActivityHeader");
            }
        }
        private string activityHeader;

        /// <summary>
        /// Gets or sets the IsSearchBarEnabled
        /// </summary>
        public bool IsSearchBarEnabled
        {
            get { return isSearchBarEnabled; }
            set
            {
                isSearchBarEnabled = value;
                RaisePropertyChanged("IsSearchBarEnabled");
            }
        }
        private bool isSearchBarEnabled = true;

        /// <summary>
        /// Gets or sets the IsSearchBarVisible
        /// </summary>
        public bool IsSearchBarVisible
        {
            get { return isSearchBarVisible; }
            set
            {
                isSearchBarVisible = value;
                RaisePropertyChanged("IsSearchBarVisible");
            }
        }
        private bool isSearchBarVisible = false;

        public IPalette Palette { get; private set; }

        // --
        // ctor

        /// <summary>
        /// Initalises a new instance of the <see cref="DCPTileSetActivity" /> class
        /// </summary>
        public DCPActivity()
            : this(null, string.Empty)
        {

        }

        /// <summary>
        /// Initalises a new instance of the <see cref="DCPTileSetActivity" /> class
        /// </summary>
        public DCPActivity(IPalette colorPalette = null)
            : this(null, string.Empty, colorPalette)
        {

        }

        /// <summary>
        /// Initalises a new instance of the <see cref="DCPTileSetActivity" /> class
        /// </summary>
        public DCPActivity(string activityName = null)
            : this(null, activityName)
        {

        }

        /// <summary>
        /// Initalises a new instance of the <see cref="DCPTileSetActivity" /> class
        /// </summary>
        public DCPActivity(string activityName = null, IPalette colorPalette = null)
            : this(null, activityName, colorPalette)
        {

        }

        /// <summary>
        /// Initalises a new instance of the <see cref="DCPTileSetActivity" /> class
        /// </summary>
        public DCPActivity(UserControl view, string activityName = null, IPalette colorPalette = null)
        {
            this.Palette = colorPalette;
            this.TileSet = new SetBuilder();
            this.View = view ?? new LayoutControl() { DataContext = TileSet };

            this.IsWorking = false;
            this.IsKillable = true; //all Killable by default

            if (string.IsNullOrEmpty(activityName))
                this.ActivityHeader = GetType().Name;
            else
                this.ActivityHeader = activityName;
        }

        // --
        // Methods


        // Activity Mangement

        /// <summary>
        /// Finishes the current Activity, performing shutdown operations.
        /// Ignores IsKillable
        /// </summary>
        protected void Finish() {
            AppHost.CloseActivity(null);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="packet"></param>
        protected void Finish(IDataPacket packet)
        {
            AppHost.CloseActivity(packet);
        }

        /// <summary>
        /// Starts new Activity
        /// </summary>
        /// <param name="activity"></param>
        public void OpenActivity(DCPActivity activity, params IDataPacket[] args)
        {
            AppHost.OpenActivity(activity, args);
        }

        /// <summary>
        /// Starts new Activity
        /// </summary>
        /// <param name="activity"></param>
        public void OpenActivity(DCPActivity activity, TileViewModel button, params IDataPacket[] args)
        {
            AppHost.OpenActivity(activity, button, args);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="app"></param>
        /// <param name="args"></param>
        public void OpenApp(DCPAppBase app, params IDataPacket[] args)
        {
            AppHost.OpenApp(app, args);
        }

        // Data Packet Management

        public IDataPacket Package<T>(T obj)
        {
            return new ObjectDataPacket<T>(obj);
        }

        public IDataPacket PackageList<T>(List<T> obj)
        {
            return new ListDataPacket<T>(obj);
        }

        public IDataPacket PackageList(params IDataPacket[] dataSet)
        {
            return new ListDataPacket<IDataPacket>(dataSet.ToList());
        }

        protected internal T Unpack<T>(IDataPacket packet)
        {
            return (packet as ObjectDataPacket<T>).Object;
        }

        protected internal List<T> UnpackList<T>(IDataPacket packet)
        {
            return (packet as ListDataPacket<T>).List;
        }

        protected internal T UnpackFrom<T>(IDataPacket packet, int index)
        {
            return Unpack<T>(UnpackList<IDataPacket>(packet)[index]);
        }

        protected internal List<T> UnpackListFrom<T>(IDataPacket packet, int index)
        {
            return UnpackList<T>(UnpackList<IDataPacket>(packet)[index]);
        }

        // Action Bar

        protected virtual void OnActionBarCreate(FixedActionBar actionBar) { }
        protected void CreateActionBar()
        {
            ActionBar = new FixedActionBar();
            OnActionBarCreate(ActionBar);
        }

        protected TileViewModel AddActionButton(ButtonTileBuilder button)
        {
            ActionBar.AddButton(button);
            return button.GetTile();
        }

        protected void SetActionBarHideable(bool isHideable = true)
        {
            ActionBar.IsHideable = isHideable;
        }

        // Tiles

        protected void SetSelectionMode(AppCore.Enums.SelectionMode selectionMode)
        {
            tileSet.DefaultCheckGroup.AllowMultipleSelection = selectionMode == Enums.SelectionMode.Multiple;
        }

        public TileViewModel AddTile(TileBuilder builder)
        {
            return TileSet.AddTile(builder.GetTile());
        }

        // Charts

        public ChartCore.ChartBaseViewModel AddChart(ChartBaseViewModel chart)
        {
            return TileSet.AddChart(chart);
        }

        public ChartCore.ChartBaseViewModel AddChart(ChartBuilder builder)
        {
            return TileSet.AddChart(builder);
        }


        public ChartCore.ChartBaseViewModel ReplaceChart(ChartCore.ChartBaseViewModel chart, ChartBuilder chartBuilder)
        {
            return TileSet.ReplaceChart(chart, chartBuilder);
        }

        public ChartCore.ChartBaseViewModel ReplaceChart(ChartCore.ChartBaseViewModel chart, ChartBaseViewModel newChart)
        {
            return TileSet.ReplaceChart(chart, newChart);
        }

        // Data Grids

        public DataGridCore.DataGridBase AddDataGrid(DataGridCore.DataGridBase datagrid)
        {
            return TileSet.AddDataGrid(datagrid);
        }

        public DataGridCore.DataGridBase AddDataGrid(DataGridBuilder builder)
        {
            return TileSet.AddDataGrid(builder);
        }

        // Monitor

        public MonitorCore.MonitorBase AddMonitor(MonitorCore.MonitorBuilder builder)
        {
            return TileSet.AddMonitor(builder);
        }

        // Canvas

        public CanvasCore.CanvasBase AddCanvas(CanvasCore.CanvasBuilder builder)
        {
            return AddCanvas(builder.GetCanvas());
        }

        public CanvasCore.CanvasBase AddCanvas(CanvasCore.CanvasBase builder)
        {
            return TileSet.AddCanvas(builder);
        }

        // Layout

        public void ClearTiles()
        {
            TileSet.Clear();
        }

        public void SkipRows(int count = 1)
        {
            TileSet.SkipRows(count);
        }

        public void SkipColumns(int count = 1)
        {
            TileSet.SkipColumns(count);
        }

        // Activity Lifecycle

        protected void SetSearchMode(SearchMode mode)
        {
            AppHost.SetupSearchBar(mode);
        }

        public virtual void OnSearchRequest(string searchRequest) { }

        protected virtual void OnResume(IDataPacket result) { this.Load(); }
        public void Resume(IDataPacket result)
        {
            this.CreateActionBar();
            this.OnResume(result);
            UnsetState(DCPSTATE.Paused);
        }

        protected virtual void OnPause() { }
        public void Pause()
        {
            OnPause();
            SetState(DCPSTATE.Paused);
        }

        protected virtual void OnCreateComplete() { return; }
        protected virtual void OnCreate(params IDataPacket[] args) { this.Load(); }
        public void Create(DCPAppBase host, params IDataPacket[] args)
         {
            this.AppHost = host;
            this.OnCreate(args);
            this.CreateActionBar();
            SetState(DCPSTATE.Created);
        }

        protected virtual void OnKill() { }
        public void Kill()
        {
            this.OnKill();
        }

        protected virtual IDataPacket LoadData() { return null; }
        protected virtual void OnLoadError(ExceptionDataPacket packet)  {   return; }
        protected virtual void OnLoadComplete(IDataPacket packet) { return; }

        private void TryDoWork(DoWorkEventArgs e, int retries = 0)
        {
            try
            {
                e.Result = LoadData();
            }
            catch (SqlException sqlExp)
            {
                // Deadlock Excpetion
                //  If we fail, try again. (max 3x)
                if (sqlExp.Number == 1205)
                {
                    if(retries > 3)
                    {
                        e.Result = new ExceptionDataPacket(sqlExp);
                        return;
                    }

                    TryDoWork(e, retries++);
                }
            }
            catch (Exception exp)
            {
                e.Result = new ExceptionDataPacket(exp);
            }
        }



        /// <summary>
        /// Initiates the Loading Cycle
        /// </summary>
        /// <param name="loadSilent"></param>
        public void Load(bool loadSilent = false)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                UnsetState(DCPSTATE.Loaded);
                IsWorking = !loadSilent;
                TryDoWork(e);
            };
            worker.RunWorkerCompleted += (s, e) =>
            {
                IsWorking = false;
                if (e.Result is ExceptionDataPacket)
                {
#if DEBUG
                    throw (e.Result as ExceptionDataPacket).Exception;
#else
                    OnLoadError(e.Result as ExceptionDataPacket);
#endif
                }
                else
                    OnLoadComplete(e.Result as IDataPacket);

                SetState(DCPSTATE.Loaded);
            };
            worker.RunWorkerAsync();
        }

        /// <summary>
        /// Reloads the View - Calls Load and appropriate Cycle
        /// </summary>
        protected void Reload() { Load(); }

        public void SetBusy(bool busy) { this.IsWorking = busy; }

        // --
        // Dialogs

        public void OpenDialog(DialogType type, string text, Action<DialogResult> callback)
        {
            AppHost.OpenDialog(type, text, callback);
        }

        public void OpenDialog(DialogType type, string text)
        {
            AppHost.OpenDialog(type, text, x => { });
        }

        public void OpenCustomDialog<ReturnType>(DCPDialog dialog, Action<DialogResult, ReturnType> callback)
        {
            AppHost.OpenDialog<ReturnType>(dialog, callback);
        }

        public override string ToString()
        {
            return ActivityHeader;
        }
    }

}
