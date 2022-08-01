using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProxyService.Database.Migrations
{
    public partial class AddEvents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"SET GLOBAL event_scheduler = ON");

            migrationBuilder.Sql(@"
                CREATE EVENT deleting_dead_proxies 
                ON SCHEDULE EVERY 1 HOUR
                STARTS now()
                DO
                UPDATE proxies 
                SET proxies.IsDeleted = 1
                WHERE 
                    proxies.Id IN (SELECT ProxyId FROM dead_proxiers);
            ");

            migrationBuilder.Sql(@"
                CREATE EVENT restoring_reborn_proxies 
                ON SCHEDULE EVERY 1 HOUR
                STARTS now()
                DO
                UPDATE proxies 
                SET proxies.IsDeleted = 0, LastChecked = NOW()
                WHERE 
                    proxies.Id IN (SELECT ProxyId FROM reborn_proxies);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP EVENT IF EXISTS restoring_reborn_proxies");

            migrationBuilder.Sql(@"DROP EVENT IF EXISTS deleting_dead_proxies");

            migrationBuilder.Sql(@"SET GLOBAL event_scheduler = OFF");
        }
    }
}
