using Pupa.BusinessObjects;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Pupa
{
    public class BaseEntity : INotifyPropertyChanged,
    INotifyPropertyChanging
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;
        protected void OnPropertyChanging([CallerMemberName] string name = null)
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(name));
        }
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private ILazyLoader _lazyLoader;

        [IgnoreDataMember, NotMapped]
        protected internal virtual ILazyLoader LazyLoader
        {
            get => _lazyLoader ?? NullLazyLoader.Instance;
            set => _lazyLoader = value;
        }
    }
}
