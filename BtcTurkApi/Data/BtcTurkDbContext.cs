using Microsoft.EntityFrameworkCore;

namespace BtcTurkApi.Data
{
    public class BtcTurkDbContext : DbContext
    {
        public DbSet<Instruction> Instructions { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(nameof(BtcTurkDbContext));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Instruction>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();                
        }
    }

    public class Instruction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int Amount { get; set; }
        public bool IsActive { get; set; }
        public int Date { get; set; }
        public bool Sms { get; set; }
        public bool Email { get; set; }
        public bool PushNotification { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
