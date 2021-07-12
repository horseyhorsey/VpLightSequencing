using Prism.Mvvm;
using System.ComponentModel;
using VpLightSequencing.Domain;

namespace VpLightSequencing.WPF.ViewModels
{
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class LightSequenceViewModel : BindableBase
    {
        public Sequence Name { get; set; }
        public int Interval { get; set; } = 10;
        public int Tail { get; set; }
        public int Repeat { get; set; } = 1;
        public int Pause { get; set; }
        public int Length { get; set; }

        public LightSequenceViewModel()
        {

        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            Length = SequenceHelper.GetSequenceLength(Name, Interval, Tail, Repeat, Pause);
            base.OnPropertyChanged(args);
        }

        /// <summary>
        /// Returns update interval and command
        /// </summary>
        /// <returns></returns>
        public string ToString(string lightSeqName = "LightSeqAttract", bool addUpdateInterval = true)
        {
            string cmd = string.Empty;
            if(addUpdateInterval) cmd = $"{lightSeqName}.UpdateInterval = {Interval}\n";
            cmd += $"{lightSeqName}.Play {Name},{Tail},{Repeat},{Pause}' total ms: {Length}";
            return cmd;
        }
    }
}
