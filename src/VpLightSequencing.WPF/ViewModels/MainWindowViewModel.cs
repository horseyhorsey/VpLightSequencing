using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace VpLightSequencing.WPF.ViewModels
{
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class MainWindowViewModel : BindableBase
    {
        private readonly IEventAggregator eventAggregator;

        #region Commands
        public DelegateCommand LoadShowCommand { get; private set; }
        public DelegateCommand SaveShowCommand { get; private set; }
        #endregion

        #region Properties
        /// <summary>
        /// Name of the object in Visual Pinball
        /// </summary>
        public string LightSeqName { get; set; } = "LightSeq001";
        public string LampshowInformation { get; set; }
        public string Script { get; set; } 
        #endregion

        [PropertyChanged.DoNotNotify]
        public ObservableCollection<LightSequenceViewModel> LightSequenceViewModels { get; set; } =
            new ObservableCollection<LightSequenceViewModel>();

        #region Constructors

        public MainWindowViewModel()
        {

        }

        public MainWindowViewModel(IEventAggregator eventAggregator)
        {
            //LightSequenceViewModel = new LightSequenceViewModel() { Name = Sequence.SeqUpOn};

            //LightSequenceViewModels.Add(LightSequenceViewModel);

            LightSequenceViewModels.CollectionChanged += LightSequenceViewModels_CollectionChanged;
            this.eventAggregator = eventAggregator;
            this.eventAggregator.GetEvent<ListUpdatedEvent>().Subscribe(() => { UpdateScript(); });

            LoadShowCommand = new DelegateCommand(OnLoadShow, () => true);
            SaveShowCommand = new DelegateCommand(OnSaveShow, CanSaveShow);
        }
        #endregion

        #region Support Methods
        private bool CanSaveShow() => LightSequenceViewModels.Count > 0;

        private void OnSaveShow()
        {
            var dialog = new SaveFileDialog()
            {
                InitialDirectory = GetOrCreateShowDirectory(),
                Filter = "Show Files(*.show)|*.show|All(*.*)|*"
            };
            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, System.Text.Json.JsonSerializer.Serialize(this));
            }
        }

        private void OnLoadShow()
        {
            try
            {
                var dialog = new OpenFileDialog()
                {
                    InitialDirectory = GetOrCreateShowDirectory(),
                    Filter = "Show Files(*.show)|*.show|All(*.*)|*"
                };
                if (dialog.ShowDialog() == true)
                {
                    using (var fs = dialog.OpenFile())
                    using (var sr = new StreamReader(fs))
                    {
                        var vm = System.Text.Json.JsonSerializer.Deserialize<MainWindowViewModel>(sr.ReadToEnd());
                        if (vm != null)
                        {
                            if (LightSequenceViewModels == null)
                            {

                            }
                            else
                            {
                                this.LightSequenceViewModels.Clear();
                            }

                            foreach (var item in vm.LightSequenceViewModels)
                            {
                                LightSequenceViewModels.Add(item);
                            }

                            LightSeqName = vm.LightSeqName;
                            Script = vm.Script;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("An error occured loading the show. " + ex.Message);
            }
        }

        /// <summary>
        /// Gets the local shows directory for loading / saving
        /// </summary>
        /// <returns></returns>
        private string GetOrCreateShowDirectory()
        {
            var currDir = Directory.GetCurrentDirectory();
            var showDir = Path.Combine(currDir, "shows");
            if (!Directory.Exists(showDir)) Directory.CreateDirectory(showDir);

            return showDir;
        }

        private void LightSequenceViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems?.Count > 0)
            {
                var s = e.NewItems[0];
                var seqVm = s as LightSequenceViewModel;
                seqVm.PropertyChanged += SeqVmPropertyChanged;
            }
            else if (e.OldItems != null)
            {
                foreach (var item in e?.OldItems)
                {
                    var sss = item as LightSequenceViewModel;
                    sss.PropertyChanged -= SeqVmPropertyChanged;
                }
            }

            SaveShowCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Updates the script when grid changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SeqVmPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateScript();
        }

        /// <summary>
        /// Updates the script text block
        /// </summary>
        internal void UpdateScript()
        {
            LampshowInformation = $"Total length: {LightSequenceViewModels.Sum(x => x.Length)}";
            Script = null;
            foreach (var item in LightSequenceViewModels)
            {
                Script += item.ToString(LightSeqName) + Environment.NewLine;
            }
        }         
        #endregion
    }
}
