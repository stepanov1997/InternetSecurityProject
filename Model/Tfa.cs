using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace InternetSecurityProject.Model
{
    [Table("tfa")]
    public class Tfa : IComparable<Tfa>
    {
        [Key] public int Id { get; set; }
        [Required]
        public User User { get; set; }
        [Required] 
        public bool IsPasswordOk { get; set; } = false;
        [Required]  
        public bool IsCertificateOk { get; set; } = false;
        [Required]  
        public bool IsTokenOk { get; set; } = false;
        [Required]
        public DateTime FirstFactorTime { get; set; }
        
        public TfaViewModel MapToModel() => new()
        {
            Username = User.Username,
            IsCertificateOk = IsCertificateOk,
            IsPasswordOk = IsCertificateOk,
            IsTokenOk = IsTokenOk
        };

        protected bool Equals(Tfa other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Tfa) obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public int CompareTo(Tfa other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Id.CompareTo(other.Id);
        }
    }

    public class TfaViewModel
    {
        public string Username { get; set; }
        public bool IsPasswordOk { get; set; }
        public bool IsCertificateOk { get; set; }
        public bool IsTokenOk { get; set; }
    }
    
    
}