namespace wcf_chess {
  using System;
  using System.Data.Entity;
  using System.ComponentModel.DataAnnotations.Schema;
  using System.Linq;

  public partial class ChessDB : DbContext {
    public ChessDB( )
        : base( "name=ChessDB" ) {
    }

    public virtual DbSet<Game> Games { get; set; }
    public virtual DbSet<Move> Moves { get; set; }
    public virtual DbSet<Player> Players { get; set; }
    public virtual DbSet<Result> Results { get; set; }
    public virtual DbSet<Side> Sides { get; set; }
    public virtual DbSet<Status> Status { get; set; }

    protected override void OnModelCreating( DbModelBuilder modelBuilder ) {
      modelBuilder.Entity<Game>( )
          .HasMany( e => e.Moves )
          .WithRequired( e => e.Game )
          .HasForeignKey( e => e.G_ID )
          .WillCascadeOnDelete( false );

      modelBuilder.Entity<Game>( )
          .HasMany( e => e.Sides )
          .WithRequired( e => e.Game )
          .HasForeignKey( e => e.G_ID )
          .WillCascadeOnDelete( false );

      modelBuilder.Entity<Move>( )
          .Property( e => e.Move_STR )
          .IsUnicode( false );

      modelBuilder.Entity<Player>( )
          .HasMany( e => e.Moves )
          .WithRequired( e => e.Player )
          .HasForeignKey( e => e.P_ID )
          .WillCascadeOnDelete( false );

      modelBuilder.Entity<Player>( )
          .HasMany( e => e.Sides )
          .WithRequired( e => e.Player )
          .HasForeignKey( e => e.P_ID )
          .WillCascadeOnDelete( false );

      modelBuilder.Entity<Result>( )
          .Property( e => e.R_Type )
          .IsUnicode( false );

      modelBuilder.Entity<Result>( )
          .HasMany( e => e.Games )
          .WithRequired( e => e.Result )
          .HasForeignKey( e => e.Game_Result )
          .WillCascadeOnDelete( false );

      modelBuilder.Entity<Side>( )
          .Property( e => e.Color )
          .IsUnicode( false );

      modelBuilder.Entity<Status>( )
          .Property( e => e.S_TYPE )
          .IsUnicode( false );

      modelBuilder.Entity<Status>( )
          .HasMany( e => e.Games )
          .WithRequired( e => e.Status )
          .HasForeignKey( e => e.Game_Status )
          .WillCascadeOnDelete( false );
    }
  }
}
