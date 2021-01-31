using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace InternetSecurityProject.Model
{
    [Table("certificate")]
    public class Certificate
    {
        [Key] public int Id { get; set; }
        [Required] public User User { get; set; }
        [Required] public string Path { get; set; }

        public CertificateViewModel MapToModel() => new()
        {
            Filename = System.IO.Path.GetFileName(Path),
            Username = User.Username,
            Content = File.ReadAllBytes(Path)
        };
    }

    public class CertificateViewModel
    {
        public string Username { get; set; }
        public string Filename { get; set; }
        public byte[] Content { get; set; }
    }
}