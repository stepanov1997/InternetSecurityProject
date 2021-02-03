using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace InternetSecurityProject.Model
{
    [Table("attack")]
    public class Attack
    {
        [Key] public int Id { get; set; }
        [Required] public User Attacker { get; set; }
        [Required] public User Attacked { get; set; }
        [Required] public DateTime DateTime { get; set; }
        [Required] public bool IsRelogged { get; set; } 
        [Required] public AttackType Type { get; set; }
    }

    public enum AttackType
    {
        SqlInjection,
        Xss,
        Ddos,
        None
    }
}