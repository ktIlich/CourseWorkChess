namespace wcf_chess
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Move")]
    public partial class Move
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Move_ID { get; set; }

        public int G_ID { get; set; }

        public int P_ID { get; set; }

        public int PLY { get; set; }

        [Required]
        [StringLength(60)]
        public string FEN { get; set; }

        [Required]
        [StringLength(10)]
        public string Move_STR { get; set; }

        public virtual Game Game { get; set; }

        public virtual Player Player { get; set; }
    }
}
