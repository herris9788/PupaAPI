using Pupa.BusinessObjects;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Pupa
{
    public class BaseEntityNumeric : INotifyPropertyChanged,
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

        [Key]
        public virtual int ID { get; set; }
        public virtual DateTime? CreatedTime { get; set; }
        public virtual int? CreatedBy { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public virtual DateTime? ModifiedTime { get; set; }
        public virtual int? ModifiedBy { get; set; }
        public virtual DateTime? DeletedTime { get; set; }
        public virtual int? DeletedBy { get; set; }
    }
}
