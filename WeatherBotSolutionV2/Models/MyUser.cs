namespace WeatherBotSolutionV2.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace WeatherBotSolutionV2.Models
    {
        public class MyUser
        {            
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }

            [Required]
            public long TelegramId { get; set; }

            [MaxLength(100)]
            public string UserName { get; set; }

            [Required]
            [MaxLength(100)]
            public string FirstName { get; set; }

            [Required]
            [MaxLength(100)]
            public string LastName { get; set; }
            [MaxLength(50)]
            public string City {  get; set; }
            public MyUser() { }

            public MyUser(long telegramId, string userName, string firstName, string lastName)
            {
                TelegramId = telegramId;
                UserName = userName ?? throw new ArgumentNullException(nameof(userName));
                FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
                LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            }

            public override string ToString()
            {
                return $"User: Id = {Id}, TelegramId = {TelegramId}, UserName = {UserName}, FirstName = {FirstName}, LastName = {LastName}";
            }

            public override bool Equals(object obj)
            {
                if (obj is MyUser other)
                {
                    return TelegramId == other.TelegramId;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return TelegramId.GetHashCode();
            }
        }
    }
}
