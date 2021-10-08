using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace julieta.Data
{
    public class Account
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }


        [Required]
        public string Login { get; set; }

        public string Name { get; set; }

        public Account() { }
    }
}