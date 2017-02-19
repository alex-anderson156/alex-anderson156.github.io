namespace AppCore
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AppCore.Components;
    using AppCore.TileCore;
    using AppCore.Tiles;
    using System.Windows.Input;
    using System.Windows.Data;
    using AppCore.Enums;
    using System.Windows;
    using AppCore.Debugger;
    using Bedrock.Code.Util;

    public abstract class DCPAppBase : DCPDialogHostViewModel
    {
        #region State Stuff

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

        #endregion

        public static DCPAppBase CurrentApp { get; private set; }

        public event EventHandler<EventArgs> OnAppCloseRequest;

        // --
        // Properties & Fields

        /// <summary>
        /// Gets or sets the Host
        /// </summary>
        public SuiteBase Host
        {
            get { return host; }
            set
            {
                host = value;
                RaisePropertyChanged("Host");
            }
        }
        private SuiteBase host;

        /// <summary>
        /// Gets or sets the Service Responsible for Generating Background notifications
        /// </summary>
        public virtual AppBackgroundNotifier NotificationService { get { return null; } }

        /// <summary>
        /// The Activity Stack
        /// </summary>
        private Stack<DCPActivity> activityStack = new Stack<DCPActivity>();

        /// <summary>
        /// Gets or sets the CurrentActivity
        /// </summary>
        public DCPActivity CurrentActivity
        {
            get { return currentActivity; }
            private set
            {
                currentActivity = value;
                RaisePropertyChanged("CurrentActivity");
                RaisePropertyChanged("MouseInActionArea");
            }
        }
        private DCPActivity currentActivity = null;

        /// <summary>
        /// Gets or sets the BackActivityCommand
        /// </summary>
        public DelegateCommand BackActivityCommand
        {
            get { return backActivityCommand; }
            set
            {
                backActivityCommand = value;
                RaisePropertyChanged("BackActivityCommand");
            }
        }
        private DelegateCommand backActivityCommand;

        /// <summary>
        /// Gets or sets the MouseActionAreaCommand
        /// </summary>
        public ICommand MouseActionAreaCommand
        {
            get { return mouseActionAreaCommand; }
            set
            {
                mouseActionAreaCommand = value;
                RaisePropertyChanged("MouseActionAreaCommand");
            }
        }
        private ICommand mouseActionAreaCommand;

        /// <summary>
        /// Gets or sets the OpenDebuggingApplicationCommand
        /// </summary>
        public ICommand OpenDebuggingApplicationCommand
        {
            get { return openDebuggingApplicationCommand; }
            set
            {
                openDebuggingApplicationCommand = value;
                RaisePropertyChanged("OpenDebuggingApplicationCommand");
            }
        }
        private ICommand openDebuggingApplicationCommand;

        /// <summary>
        /// Gets or sets the MouseInActionArea
        /// </summary>
        public bool MouseInActionArea
        {
            get
            {
                if (CurrentActivity == null)
                    return true;

                if (CurrentActivity.ActionBar.IsHideable)
                    return mouseInActionArea;

                return true;
            }
            set
            {
                mouseInActionArea = value;
                RaisePropertyChanged("MouseInActionArea");
            }
        }
        private bool mouseInActionArea = false;

        /// <summary>
        /// Gets or sets the IsWorking
        /// </summary>
        public bool IsWorking
        {
            get { return isWorking; }
            set
            {
                isWorking = value;
                RaisePropertyChanged("IsWorking");
            }
        }
        private bool isWorking;

        /// <summary>
        /// Gets or sets the LoadingContent
        /// </summary>
        public string LoadingInformation
        {
            get { return loadingInformation; }
            set
            {
                loadingInformation = value;
                RaisePropertyChanged("LoadingInformation");
            }
        }
        private string loadingInformation = "Loading";

        /// <summary>
        /// Gets or sets the IsBackCommandEnabled
        /// </summary>
        public bool IsBackCommandEnabled
        {
            get
            {
                return isBackCommandEnabled;
            }
            set
            {
                isBackCommandEnabled = value;
                RaisePropertyChanged("IsBackCommandEnabled");
            }
        }
        private bool isBackCommandEnabled = true;

        /// <summary>
        /// Gets or sets the ToggleSearchBoxCommand
        /// </summary>
        public ICommand ToggleSearchBoxCommand
        {
            get { return toggleSearchBoxCommand; }
            set
            {
                toggleSearchBoxCommand = value;
                RaisePropertyChanged("ToggleSearchBoxCommand");
            }
        }
        private ICommand toggleSearchBoxCommand;

        /// <summary>
        /// Gets or sets the SearchBoxText
        /// </summary>
        public string SearchBoxText
        {
            get { return searchBoxText; }
            set
            {
                searchBoxText = value;
                RaisePropertyChanged("SearchBoxText");

                if (CurrentActivity != null)
                    CurrentActivity.OnSearchRequest(value);
            }
        }
        private string searchBoxText;

        /// <summary>
        /// Gets or sets the SeachBarBinding
        /// </summary>
        public Binding SeachBarBinding
        {
            get { return seachBarBinding; }
            set
            {
                seachBarBinding = value;
                RaisePropertyChanged("SeachBarBinding");
            }
        }
        private Binding seachBarBinding;

        /// <summary>
        /// Gets or sets the HomeCommand
        /// </summary>
        public ICommand HomeCommand
        {
            get { return homeCommand; }
            set
            {
                homeCommand = value;
                RaisePropertyChanged("HomeCommand");
            }
        }
        private ICommand homeCommand;

        /// <summary>
        /// Event firing to notify the system that an activity has gone ito sleep mode.
        /// </summary>
        public event EventHandler<GenericEventArgs<DCPActivity, TileViewModel>> OnActivityEnterSleep;

        /// <summary>
        /// Event Firing to notify the system that an Activity has been awoken from sleep Mode
        ///   used to Notify the breadcrumb system that a Breadcrumb should be destoryed.
        /// </summary>
        public event EventHandler<GenericEventArgs<DCPActivity>> OnActivityAwakening;

        private IDataPacket[] CreatedArguments { get; set; }

        private DCPActivity InitalActivity { get; set; }

        // --
        // ctor

		/// <summary>
		/// Initalises a new instance of the <see cref="DCPAppBase" /> class
		/// </summary>
		public DCPAppBase() {
            this.MouseActionAreaCommand = CreateDelegate<bool>(inArea => { MouseInActionArea = inArea; });
            this.IsBackCommandEnabled = true;

            this.HomeCommand = CreateDelegate(_ => {  Host.HomepageApp(); });
            this.ToggleSearchBoxCommand = CreateDelegate(_ => { CurrentActivity.IsSearchBarVisible = !CurrentActivity.IsSearchBarVisible; });

            this.SeachBarBinding = new Binding()
            {
                Path = new PropertyPath("SearchBoxText"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            };

            this.OpenDebuggingApplicationCommand = CreateDelegate(_ =>
            {
                DCPDebugger.Open();
            });
	    }

        // --
        // Methods

        /// <summary>
        /// Opens the specified activity with arguments
        /// </summary>
        /// <param name="activity">The Activity to Open</param>
        /// <param name="args">Optional Parameters to pass to the openening activity.</param>
        public void OpenActivity(DCPActivity activity, params IDataPacket[] args) {
            OpenActivity(activity, null, args);
        }

        /// <summary>
        /// Opens the specified activity with arguments
        /// </summary>
        /// <param name="activity">The Activity to Open</param>
        /// <param name="button">A Tile Button that opened the Tile (USed in breadcrumb rendering)</param>
        /// <param name="args">Optional Parameters to pass to the openening activity.</param>
        public void OpenActivity(DCPActivity activity, TileViewModel button, params IDataPacket[] args)
        {
            DCPActivity sleepingActivity = null;
            // If there is an Activity Open
            if (activityStack.Count > 0) {
                sleepingActivity = activityStack.Peek();
                sleepingActivity.Pause(); // Pause it

                if (OnActivityEnterSleep != null)
                    OnActivityEnterSleep(this, new GenericEventArgs<DCPActivity, TileViewModel>(sleepingActivity, button));
            }

            activityStack.Push(activity);
            activity.Create(this, args);
            CurrentActivity = activityStack.Peek();

            //IsBackCommandEnabled = activityStack.Count > 1;
        }

        public void OpenApp(DCPAppBase app, params IDataPacket[] args)
        {
            Host.SetApp(app, args);
        }

        /// <summary>
        /// Closes the top Activity on the Stack, passing the results back to the previous actiivtys OnResume()
        /// </summary>
        /// <param name="result">(Optional)The Result of the previous activity, passed back to the last activity OnResume()</param>
        public void CloseActivity(IDataPacket result = null)
        {
            //Get the Closing activity, Ignore Killable.
            DCPActivity topOfStack = activityStack.Pop();
            topOfStack.Kill();

            if (activityStack.Count == 0) {
                //
                if (OnAppCloseRequest != null)
                    OnAppCloseRequest(this, EventArgs.Empty);

                return;
            }

            //Get the awakening app
            DCPActivity awakeningApp = activityStack.Peek();
            CurrentActivity = awakeningApp;

            //Message the breadcrumb system with the awakening app
            if (OnActivityAwakening != null)
                OnActivityAwakening(this, new GenericEventArgs<DCPActivity>(awakeningApp));

            awakeningApp.Resume(result);
            //IsBackCommandEnabled = activityStack.Count > 1;
        }

        protected virtual IDataPacket LoadData() { return null; }
        protected virtual void OnLoadError(ExceptionDataPacket packet) { return; }
        protected virtual void OnLoadComplete(IDataPacket packet) { return; }
        public void Load()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                IsWorking = true;

                try
                {
                    IsWorking = true;
                    e.Result = LoadData();
                    //Thread.Sleep(100);
                }
                catch (Exception exp)
                {
                    e.Result = new ExceptionDataPacket(exp);
                }
            };
            worker.RunWorkerCompleted += (s, e) =>
            {
                IsWorking = false;
                if (e.Result is ExceptionDataPacket)
                    OnLoadError(e.Result as ExceptionDataPacket);
                else
                    OnLoadComplete(e.Result as IDataPacket);

                SetState(DCPSTATE.Loaded);
            };
            worker.RunWorkerAsync();
        }

        protected void OnCreateComplete()
        {
            // Once we are Loaded & Created
            this.OpenActivity(InitalActivity, CreatedArguments);
        }
        protected abstract DCPActivity OnCreate(params IDataPacket[] args);
        public void Create(SuiteBase suite, params IDataPacket[] args)
        {
            //Set the Current App
            this.Host = suite;
            DCPAppBase.CurrentApp = this;

            // Initalise / reINitalse the App
            this.ActivityState = 0;
            this.CurrentActivity = null;
            this.InitalActivity = null;
            this.CreatedArguments = null;
            this.BackActivityCommand = CreateDelegate(OnBackActivity);
            this.activityStack.Clear(); // just destroy.
            this.OnActivityEnterSleep = null;
            this.OnActivityAwakening = null;
            this.OnAppCloseRequest = null;

            //Start the Load Cycle
            this.Load();

            //Create the Inital Activity
            this.CreatedArguments = args;
            this.InitalActivity = this.OnCreate(args);
            SetState(DCPSTATE.Created);
        }

        public void SetupSearchBar(SearchMode mode)
        {
            SeachBarBinding = new Binding()
            {
                Path = new PropertyPath("SearchBoxText"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = mode == SearchMode.Reactive ? UpdateSourceTrigger.PropertyChanged : UpdateSourceTrigger.LostFocus,
            };
        }

        protected virtual bool BeforeKill() { return true; }
        protected virtual void OnKill() {  }

        /// <summary>
        /// Sends a Kill message to the Application
        /// </summary>
        /// <returns>A Result indicating wether or no the application can be killed.</returns>
        public bool Kill()
        {
            if (activityStack.All(x => x.IsKillable))
            {
                if (!this.BeforeKill())
                    return false;

                // All Activities are Killable
                // Kill
                while (activityStack.Count > 0) {
                    activityStack.Pop().Kill();
                }

                this.OnKill();
            }

            return true;
        }

        /// <summary>
        /// Navigates Back in the Chain to an already open activity.
        /// </summary>
        /// <param name="toActivity"></param>
        internal void NavigateBackToActivity(DCPActivity toActivity)
        {
            // Look back accros the stack & see if there is nothing blocking us from being killed.
            DCPActivity[] stack = activityStack.ToArray();

            // Check for Immortal Activities
            foreach (DCPActivity activity in stack)
            {
                if (activity == toActivity)
                    break; // We found it, Just Skip to Killing part
                else if (!activity.IsKillable)
                {
                    // Activity before the one i want to navigate too is Immortal.
                    //  show an Warning dialog showing that an unkillable activity is blocking navigation
                    OpenDialog(DialogType.Warning,
                        "An Activity is currently waiting for information before it can be closed. Do you want to Open this activity?",
                        option =>
                        {
                            if (option == DialogResult.Yes)
                                this.KillBackStackApps(toActivity);
                        });

                    return;
                }
            }

            this.KillBackStackApps(toActivity);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="toActivity"></param>
        private void KillBackStackApps(DCPActivity toActivity)
        {
            // Killing Part
            while (true)
            {
                // Look at the top activity from the stack
                DCPActivity top = activityStack.Peek();

                // if the top activity is the one we want to navigate too
                if (top == toActivity)
                {
                    break;
                }
                else
                {
                    // If the Activity is not killable
                    if (!top.IsKillable)
                    {
                        break;
                    }
                    else // else kill it.
                        CloseActivity();
                }

            }
        }

        /// <summary>
        /// Navigates back 1 Activity in the stack.
        /// </summary>
        /// <param name="obj"></param>
        private void OnBackActivity(object obj)
        {
            this.CloseActivity();
        }

        // --
        // Dialogs

        public void OpenDialog(DialogType type, string text, Action<DialogResult> callback)
        {
            OpenDialog<DialogResult>(new DefaultMessageDialogViewModel(type, text), (diagResult, result) => { callback(diagResult); });
        }

        public void OpenDialog(DialogType type, string text)
        {
            OpenDialog<DialogResult>(new DefaultMessageDialogViewModel(type, text), (diagResult, result) => {  });
        }

    }
}
