namespace wcf_chess
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Side")]
    public partial class Side
    {
        [Key]
        public int SIDE_ID { get; set; }

        public int G_ID { get; set; }

        public int P_ID { get; set; }

        [Required]
        [StringLength(10)]
        public string Color { get; set; }

        public int? Win_W { get; set; }

        public int? Win_B { get; set; }

        public virtual Game Game { get; set; }

        public virtual Player Player { get; set; }
    }
}
