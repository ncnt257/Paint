using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paint
{
    public class ColorModel : INotifyPropertyChanged
    {
        public string test { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged = null;
    }
}
