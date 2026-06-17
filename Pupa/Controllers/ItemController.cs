using Pupa.BusinessObjects.Beesuite;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Pupa.BusinessObjects;

namespace Pupa.Controllers
{
    [Route("beesuite/api/[controller]")]
    public class ItemController : Controller
    {
        private readonly BeesuiteDbContext _db;

        public ItemController(BeesuiteDbContext db)
        {
            _db = db;
        }

        public class EquivalentItemDto
        {
            public virtual string ItemCode { get; set; }
            public virtual string Brand { get; set; }
            public virtual string Description { get; set; }
            public virtual string GroupName { get; set; }
            public virtual string Category { get; set; }
        }

        [HttpGet("Equivalent")]
        public async Task<IActionResult> GetEquivalent([FromQuery] string itemCode)
        {
            try
            {
                var conn = _db.Database.GetDbConnection();
                if (conn.State != ConnectionState.Open)
                    await conn.OpenAsync();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT * FROM ""GetEquivalents""(@ItemCode)";
                var param = cmd.CreateParameter();
                param.ParameterName = "@ItemCode";
                param.Value = itemCode;
                cmd.Parameters.Add(param);
                using var reader = await cmd.ExecuteReaderAsync();
                var result = new List<EquivalentItemDto>();
                while (await reader.ReadAsync())
                {
                    result.Add(new EquivalentItemDto
                    {
                        ItemCode = reader["ItemCode"]?.ToString(),
                        Brand = reader["Brand"]?.ToString(),
                        Description = reader["Description"]?.ToString(),
                        GroupName = reader["GroupName"]?.ToString(),
                        Category = reader["Category"]?.ToString(),
                    });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
