using Contract;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paint
{
    public class Layer : INotifyPropertyChanged
    {
        public bool isChecked { get; set; }
        public List<IShape> _shapes { get; set; }
        public int index { get; set; }
        public Layer(int index,bool isChecked=false)
        {
            this.index = index;
            this.isChecked = isChecked;
            _shapes = new List<IShape>();
        }

        public event PropertyChangedEventHandler? PropertyChanged = null;
    }
}
