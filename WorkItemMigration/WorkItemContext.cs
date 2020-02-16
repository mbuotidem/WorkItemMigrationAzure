using Microsoft.EntityFrameworkCore;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime;
using Microsoft.VisualStudio.Services.WebApi;

namespace WorkItemMigration
{
    public class WorkItemContext : DbContext
    {
        public DbSet<DWWorkItem> WorkItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer(@"Server=.\SQLEXPRESS;Database=SchoolDB;Trusted_Connection=True;");
            optionsBuilder.UseSqlServer(@"Server=HCV000C29597DCA;Database=WorkReports;Trusted_Connection=True;");
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DWWorkItem>().Property(a => a.Id).ValueGeneratedNever();
            modelBuilder.Entity<DWWorkItem>().Property(a => a.DWId).ValueGeneratedOnAdd();

            modelBuilder.Entity<WorkItem>().Ignore(c => c.CommentVersionRef);
            //modelBuilder.Entity<WorkItem>().Ignore(c => c.Relations);
            modelBuilder.Entity<WorkItem>().Ignore(c => c.Links);
            modelBuilder.Entity<WorkItem>(b =>
            {
                b.Property(u => u.Fields)
                    .HasConversion(
                        d => JsonConvert.SerializeObject(d, Formatting.None),
                        s => JsonConvert.DeserializeObject<IDictionary<string, object>>(s)
                    )
                    //.HasMaxLength(4000)
                    .IsRequired();
            });

            modelBuilder.Entity<WorkItem>(b =>
            {
                b.Property(u => u.Relations)
                    .HasConversion(
                        d => JsonConvert.SerializeObject(d, Formatting.None),
                        s => JsonConvert.DeserializeObject<IList<WorkItemRelation>>(s)
                    )
                    //.HasMaxLength(4000)
                    .IsRequired();
            });


            //modelBuilder.Entity<WorkItemRelation>(b =>
            //{
            //    b.Property(u => u.Attributes)
            //        .HasConversion(
            //            d => JsonConvert.SerializeObject(d, Formatting.None),
            //            s => JsonConvert.DeserializeObject<IDictionary<string, object>>(s)
            //        )
            //        .HasMaxLength(4000)
            //        .IsRequired();
            //});

            //modelBuilder.Entity<ReferenceLinks>(eb =>
            //{
            //    eb.HasNoKey();
            //}
            //    );


            //modelBuilder.Entity<WorkItemRelation>(eb =>
            //{
            //    eb.HasNoKey();
            //}
            //    );
            //modelBuilder.Entity<WorkItemCommentVersionRef>(eb =>
            //{
            //    eb.HasNoKey();
            //}
            //    );



        }
    }
}
