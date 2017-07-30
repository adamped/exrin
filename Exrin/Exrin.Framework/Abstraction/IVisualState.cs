﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exrin.Abstraction
{
    public interface IVisualState: INotifyPropertyChanged
    {
		void Init();

        bool IsBusy { get; set; }
    }
}
