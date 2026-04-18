using Marilog.Domain.Entities.LaytimeEntities;
using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class CharterTermsConfiguration : IEntityTypeConfiguration<CharterTerms>
    {
        public void Configure(EntityTypeBuilder<CharterTerms> builder)
        {
            builder.ToTable("CharterTerms");

            // ─────────────────────────────
            // Primary Key
            // ─────────────────────────────
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .ValueGeneratedOnAdd();

            // ─────────────────────────────
            // FK (1:1 Required Relationship)
            // ─────────────────────────────
            builder.Property(x => x.ContractId)
                   .IsRequired();

            builder.HasIndex(x => x.ContractId)
                   .IsUnique();

            builder.HasOne<AContract>()
                   .WithOne(c => c.CharterTerms)
                   .HasForeignKey<CharterTerms>(x => x.ContractId)
                   .OnDelete(DeleteBehavior.Cascade);

            // ─────────────────────────────
            // Scalar Property
            // ─────────────────────────────
            builder.Property(x => x.CargoQuantityMt)
                   .HasPrecision(18, 3)
                   .IsRequired();

            // ─────────────────────────────
            // LaytimeTerms (Owned Graph Root)
            // ─────────────────────────────
            builder.OwnsOne(x => x.LaytimeTerms, lt =>
            {
                lt.Navigation(x => x.Loading).IsRequired();
                lt.Navigation(x => x.Discharging).IsRequired();
                lt.Navigation(x => x.Demurrage).IsRequired();
                lt.Navigation(x => x.Despatch).IsRequired();
                lt.Navigation(x => x.RuleOptions).IsRequired();

                // ───────── Loading ─────────
                lt.OwnsOne(t => t.Loading, lo =>
                {
                    lo.Property(p => p.RateMtPerDay)
                      .HasColumnName("Loading_RateMtPerDay")
                      .HasPrecision(18, 3);

                    lo.Property(p => p.CalendarType)
                      .HasConversion<string>()
                      .HasColumnName("Loading_CalendarType")
                      .HasMaxLength(30);

                    lo.Property(p => p.NoticeHours)
                      .HasColumnName("Loading_NoticeHours");

                    lo.Property(p => p.IsReversible)
                      .HasColumnName("Loading_IsReversible");

                    lo.Property(p => p.IsWeatherWorkingDay)
                      .HasColumnName("Loading_IsWeatherWorkingDay");

                    lo.Property(p => p.OperationType)
                      .HasConversion<string>()
                      .HasColumnName("Loading_OperationType")
                      .HasMaxLength(20);
                });

                // ───────── Discharging ─────────
                lt.OwnsOne(t => t.Discharging, di =>
                {
                    di.Property(p => p.RateMtPerDay)
                      .HasColumnName("Discharging_RateMtPerDay")
                      .HasPrecision(18, 3);

                    di.Property(p => p.CalendarType)
                      .HasConversion<string>()
                      .HasColumnName("Discharging_CalendarType")
                      .HasMaxLength(30);

                    di.Property(p => p.NoticeHours)
                      .HasColumnName("Discharging_NoticeHours");

                    di.Property(p => p.IsReversible)
                      .HasColumnName("Discharging_IsReversible");

                    di.Property(p => p.IsWeatherWorkingDay)
                      .HasColumnName("Discharging_IsWeatherWorkingDay");

                    di.Property(p => p.OperationType)
                      .HasConversion<string>()
                      .HasColumnName("Discharging_OperationType")
                      .HasMaxLength(20);
                });

                // ───────── Demurrage ─────────
                lt.OwnsOne(t => t.Demurrage, d =>
                {
                    d.Property(p => p.RateUsdPerDay)
                     .HasColumnName("Demurrage_RateUsdPerDay")
                     .HasPrecision(18, 2);

                    d.Property(p => p.OnceOnDemurrageAlwaysOnDemurrage)
                     .HasColumnName("Demurrage_OnceAlwaysOnDemurrage");
                });

                // ───────── Despatch ─────────
                lt.OwnsOne(t => t.Despatch, d =>
                {
                    d.Property(p => p.RateUsdPerDay)
                     .HasColumnName("Despatch_RateUsdPerDay")
                     .HasPrecision(18, 2);

                    d.Property(p => p.Basis)
                     .HasConversion<string>()
                     .HasColumnName("Despatch_Basis")
                     .HasMaxLength(30);
                });

                // ───────── Rules ─────────
                lt.OwnsOne(t => t.RuleOptions, ro =>
                {
                    ro.Property(p => p.DraftSurveyCounts)
                      .HasColumnName("Rules_DraftSurveyCounts");

                    ro.Property(p => p.HolidaysIncluded)
                      .HasColumnName("Rules_HolidaysIncluded");

                    ro.Property(p => p.SundaysIncluded)
                      .HasColumnName("Rules_SundaysIncluded");

                    ro.Property(p => p.TimeReversible)
                      .HasColumnName("Rules_TimeReversible");

                    ro.Property(p => p.AllowShiftingTime)
                      .HasColumnName("Rules_AllowShiftingTime");
                });
            });

            //---Indexing-----
            builder.HasIndex(x => x.ContractId).IsUnique();
        }
    }
}
