using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProxyService.Database.Migrations
{
	/// <inheritdoc />
	public partial class AddCustomViews : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(@"
            CREATE VIEW last_4_runs AS
            SELECT 
                checking_runs.Id AS RunId 
            FROM checking_runs 
            WHERE 
                checking_runs.Ignore = 0
            HAVING 
                (SELECT COUNT(*) FROM checking_runs) > 4
            ORDER BY checking_runs.Created DESC 
            LIMIT 4 
            OFFSET 1
            ");

			migrationBuilder.Sql(@"
            CREATE VIEW checking_sessions_from_last_4_runs AS
            SELECT 
                checking_sessions.Id AS SessionId
            FROM checking_sessions 
            WHERE 
                checking_sessions.CheckingRunId IN (SELECT * FROM last_4_runs) AND
                checking_sessions.Ignore = 0
            ");

			migrationBuilder.Sql(@"
            CREATE VIEW dead_proxiers AS
            SELECT 
                checking_results.ProxyId as ProxyId
            FROM checking_results 
            INNER JOIN proxies ON  proxies.Id = checking_results.ProxyId
            WHERE 
                checking_results.CheckingSessionId IN (SELECT * FROM checking_sessions_from_last_4_runs) AND 
                checking_results.Result = 0 AND
                checking_results.Ignore = 0 AND
                proxies.IsDeleted = 0
            GROUP BY checking_results.ProxyId 
            HAVING 
                COUNT(checking_results.Id) = (SELECT COUNT(*) FROM checking_sessions_from_last_4_runs)
            ");

			migrationBuilder.Sql(@"
            CREATE VIEW reborn_proxies as
            SELECT 
                checking_results.ProxyId as ProxyId
            FROM checking_results 
            INNER JOIN proxies ON proxies.Id = checking_results.ProxyId
            WHERE 
                checking_results.CheckingSessionId IN (SELECT * FROM checking_sessions_from_last_4_runs) AND 
                checking_results.Result = 1 AND
                proxies.IsDeleted = 1
            GROUP BY checking_results.ProxyId
            ");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(@"DROP VIEW reborn_proxies");

			migrationBuilder.Sql(@"DROP VIEW dead_proxiers");

			migrationBuilder.Sql(@"DROP VIEW checking_sessions_from_last_4_runs");

			migrationBuilder.Sql(@"DROP VIEW last_4_runs");
		}
	}
}
