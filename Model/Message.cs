using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternetSecurityProject.Model
{
    [Table("message")]
    public class Message
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public User Sender { get; set; }        
        [Required]
        public User Receiver { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public DateTime DateTimeStamp { get; set; }
        public DateTime SeenDateTime { get; set; }
        public dynamic MapToModel()
        {
            return new
            {
                Id = Id,
                Sender = Sender.Username,
                Receiver = Receiver.Username,
                Content = Content,
                DateTimeStamp = DateTimeStamp,
                SeenDateTime = SeenDateTime
            };
        }
    }

    public class MessageModel
    {
        public string Content { get; set; }
        public string Sender { get; set; }
        public string Receiver { get; set; }
    }
}