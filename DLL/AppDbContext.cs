using Microsoft.EntityFrameworkCore;
using Spendwise_WebApp.Models;

namespace Spendwise_WebApp.DLL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Package> packages { get; set; }
        public virtual DbSet<PackageFeature> PackageFeatures { get; set; }
        public virtual DbSet<AdditionalPackageItem> AdditionalPackageItems { get; set; }
        public virtual DbSet<Orders> Orders { get; set; }
        public virtual DbSet<CompanyDetail> CompanyDetails { get; set; }
        public virtual DbSet<CompanyOffice> CompanyOffices { get; set; }
        public virtual DbSet<CompanyOfficer> CompanyOfficers { get; set; }
        public virtual DbSet<AddressData> AddressData { get; set; }
        public virtual DbSet<Particular> Particulars { get; set; }
        public virtual DbSet<SicCodeCategory> SicCodeCategory { get; set; }
        public virtual DbSet<SicCodes> SicCodes { get; set; }
        public virtual DbSet<Document> Documents { get; set; }
        public virtual DbSet<CorporateCompanyOfficers> CorporateCompanyOfficers { get; set; }
        public virtual DbSet<OtherLegalOfficers> OtherLegalOfficers { get; set; }

    }
}
