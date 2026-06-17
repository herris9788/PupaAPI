using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities.Encoders;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace Pupa.Helpers
{
    public class SecurityHelper
    {
        public static string GenerateJWT(string UserName, List<string> Roles, List<Claim> Claims = null)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.JWTSecret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, UserName));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            foreach (string role in Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            if (Claims != null)
            {
                claims.AddRange(Claims);
            }
            var token = new JwtSecurityToken(
                issuer: Configuration.JWTIssuer,
                audience: Configuration.JWTIssuer,
                claims,
                expires: DateTime.Now.AddSeconds(10),
                signingCredentials: credentials);
            var encodetoken = new JwtSecurityTokenHandler().WriteToken(token);
            return encodetoken;
        }
        public static string GenerateJWT(string UserName, List<Claim> Claims = null)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.JWTSecret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, UserName));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            if (Claims != null)
            {
                claims.AddRange(Claims);
            }
            var token = new JwtSecurityToken(
                issuer: Configuration.JWTIssuer,
                audience: Configuration.JWTIssuer,
                claims,
                expires: DateTime.Now.AddSeconds(10),
                signingCredentials: credentials);
            var encodetoken = new JwtSecurityTokenHandler().WriteToken(token);
            return encodetoken;
        }
        public static string GetClaim(string Token, string Key)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(Token);
            var Username = jwt.Claims.First(claim => claim.Type == Key).Value;
            return Username;
        }

        public static List<Claim> GetClaims(string Token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(Token);
            return jwt.Claims.ToList();
        }
        public static string Encrypt(string Password)
        {
            AESEnDecryption AESEnDecryption = new AESEnDecryption();
            return AESEnDecryption.EncryptStrAndToBase64(Password, Configuration.AESSecretKey, Configuration.AESIVKey);
        }
        public static string Decrypt(string PasswordHash)
        {
            AESEnDecryption AESEnDecryption = new AESEnDecryption();
            return AESEnDecryption.DecryptStrAndFromBase64(PasswordHash, Configuration.AESSecretKey, Configuration.AESIVKey);
        }
        public static string GetUserNameFromAuthorization(string Token)
        {
            string UserName = Decrypt(GetClaim(Token, JwtRegisteredClaimNames.Sub));
            return UserName;
        }
        public static List<string> GetAllRoleTypes(Type type)
        {
            return type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
                .Select(x => (string)x.GetRawConstantValue())
                .ToList();
        }

        public static List<RoleType> GetAllRoleTypeAndDescription(Type type)
        {
            return type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
                .Select(x => new RoleType() { Name = x.Name, Description = (string)x.GetRawConstantValue() })
                .ToList();
        }
        public static List<RoleTypeAscend> GetAllRoleTypeAndDescriptionAscend(Type type)
        {
            return type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
                .Select(x => new RoleTypeAscend() { Name = x.Name, Description = (string)x.GetRawConstantValue(), RoleName = x.Name })
                .ToList();
        }
        public static string EncryptRole(string role)
        {
            Guid guid = CryptoService.ComputeGuid(role.Trim().ToLower());
            string str2 = guid.ToString();
            string str1 = "";
            str1 = str1 + (str1.Length > 0 ? "|" : "") + str2;
            return str1;
        }


        public static List<string> romanNumerals = new List<string>() { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
        public static List<int> numerals = new List<int>() { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };

        public static string ToRomanNumeral(int number)
        {
            var romanNumeral = string.Empty;
            while (number > 0)
            {
                // find biggest numeral that is less than equal to number
                var index = numerals.FindIndex(x => x <= number);
                // subtract it's value from your number
                number -= numerals[index];
                // tack it onto the end of your roman numeral
                romanNumeral += romanNumerals[index];
            }
            return romanNumeral;
        }
        public static string GetMonthRomance(int month)
        {
            return ToRomanNumeral(month);
        }
        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                return Convert.ToHexString(hashBytes); // .NET 5 +

                // Convert the byte array to hexadecimal string prior to .NET 5
                // StringBuilder sb = new System.Text.StringBuilder();
                // for (int i = 0; i < hashBytes.Length; i++)
                // {
                //     sb.Append(hashBytes[i].ToString("X2"));
                // }
                // return sb.ToString();
            }
        }
        public static string QueryOustandingMR(string Company)
        {
            return @$"
            SELECT '{Company}' as Company,
                    (SELECT TOP 1 InventoryUserName FROM API.Ascend.Vessel x WHERE (SELECT TOP 1 RequiredByID FROM AS_{Company}.dbo.AP_MRDetails WHERE MRID = MR.MRID) = x.InventoryUserID AND x.CompanyCode = '{Company}') AS Vessel,
                    (SELECT TOP 1 Fleet FROM API.Ascend.Vessel x WHERE (SELECT TOP 1 RequiredByID FROM AS_{Company}.dbo.AP_MRDetails WHERE MRID = MR.MRID) = x.InventoryUserID AND x.CompanyCode = '{Company}') AS Fleet,
                    MR.MRID AS MRID,
                    MR.MRNumber AS MRNumber,
                    MR.MRCounter AS MRCounter,
                    MR.Class,
                    MR.Date,
                    MR.InventoryUserId AS InventoryUserID,
                    MR.Importance,
                    MR.RequiredDate,
                    MR.Void,
                    MR.VoidDateTime,
                    MR.VoidBy,
                    MR.VoidReason,
                    MR.Remarks AS MRHeader_Remarks,
                    MR.CreatedBy,
                    MR.CreateDate,
                    MR.LastModBy,
                    MR.LastMod,
                    MR.MRType,
                    MR.Approved,
                    MR.Purpose AS MRHeader_Purpose,
                    MR.ExternalRef,
                    MR.MR1stRequired AS MR1stRequired,
                    MR.MR1stApprovedBy AS MR1stApprovedBy,
                    MR.MR1stApprovedDateTime AS MR1stApprovedDateTime,
                    MR.MR2ndRequired AS MR2ndRequired,
                    MR.MR2ndApprovedBy AS MR2ndApprovedBy,
                    MR.MR2ndApprovedDateTime AS MR2ndApprovedDateTime,
                    MR.MR3rdRequired AS MR3rdRequired,
                    MR.MR3rdApprovedBy AS MR3rdApprovedBy,
                    MR.MR3rdApprovedDateTime AS MR3rdApprovedDateTime,
                    MR.MR4thRequired AS MR4thRequired,
                    MR.MR4thApprovedBy AS MR4thApprovedBy,
                    MR.MR4thApprovedDateTime AS MR4thApprovedDateTime,
                    MR.ProjectID,
                    MR.Closed,
                    MR.ClosedBy,
                    MR.ClosedDateTime,
                    MR.ClosedReason,
                    MR.IsReplacement,
                    MR.IsDraft,
                    MR.IsService,
                    MR.ServiceDeptID AS ServiceDeptID,
                    MR.AssetID AS AssetID,
                    MR.Problem,
                    MR.KMHM,
                    MR.ServiceAction,
                    MR.CompletedBy,
                    MR.CompletedDateTime,
                    MR.CompletedComment,
                    MR.UatBy,
                    MR.UatDateTime,
                    MR.UatComment,
                    MR.SubmitDateTime,
                    MR.DelegateToMRType AS DelegateToMRType,
                    MR.MCostAs,
                    MR.RefDate,
                    MR.SPWarehouseID AS SPWarehouseID,
                    MR.ExSource,
                    MR.ExSourceID AS ExSourceID,
                    MR.ForceClose,
                    MR.SvcMode,
                    MR.SvcItemID AS SvcItemID,
                    MR.SvcOther,
                    MR.Mode,
                    MR.MRServiceDeptID AS MRServiceDeptID,
                    MR.PLV2ItemID AS PLV2ItemID,
                    MR.Score,
                    MR.ForBAK,
                    MR.SvcPic,

                    MRD.MRDetailID,
                    MRD.MRID AS MRDetail_MRID,
                    MRD.ItemID AS MRDetail_ItemID,
                    MRD.UOMLevel,
                    MRD.Quantity,
                    MRD.Remarks AS MRDetail_Remarks,
                    MRD.RequiredByID,
                    MRD.GlobalQuantity,
                    MRD.GlobalUOMLevel,
                    MRD.Purpose AS MRDetail_Purpose,
                    MRD.DisplayIndex,
                    MRD.ItemDescription,
                    MRD.UOMID,
                    MRD.ItemRequiredDate,
                    MRD.DetailGuid,
                    MRD.DetailVoid,
                    MRD.DetailVoidBy,
                    MRD.DetailVoidDateTime,
                    MRD.DetailVoidReason,
                    MRD.WarehouseQuantity,
                    MRD.Comment,
                    MRD.DetailClosed,
                    MRD.DetailClosedBy,
                    MRD.DetailCloseReason,
                    MRD.DetailCostCenter,
                    MRD.QuantityRequestedExtra,
                    MRD.Booking,
                    MRD.Category,
                    MRD.BQDetailID,
                    MRD.RsvWarehouseID,
                    MRD.CostAs,
                    MRD.OldItemID,
                    MRD.DetailDelegateToMRType,
                    MRD.DetailDelegateReason,
                    MRD.AllowOverBQ,
                    MRD.AllowPRInc,
                    MRD.BookedDate,
                    MRD.DJobID,

                    I.ItemID AS ItemID,
                    I.ItemCode,
                    I.ItemName,
                    I.Specification,
                    I.PartNoEx,
                    I.UOMID1,
                    I.UOMID2,
                    I.UOMID3,
                    I.UOMID4,
		             (
			            CASE WHEN (MRD.UOMLevel = 1) 
			            THEN 
				            (SELECT UOMCode FROM IC_UOM WHERE UOMID = I.UOMID1)
			            WHEN (MRD.UOMLevel = 2)
			            THEN
				            (SELECT UOMCode FROM IC_UOM WHERE UOMID = I.UOMID2)
			            WHEN (MRD.UOMLevel = 3)
			            THEN
				            (SELECT UOMCode FROM IC_UOM WHERE UOMID = I.UOMID3)
			            WHEN (MRD.UOMLevel = 4)
			            THEN
				            (SELECT UOMCode FROM IC_UOM WHERE UOMID = I.UOMID4)
			            ELSE
				            ''
			            END
		            )  AS UOMCode,
		            (
			            CASE WHEN (MRD.UOMLevel = 1) 
			            THEN 
				            (SELECT Decimals FROM IC_UOM WHERE UOMID = I.UOMID1)
			            WHEN (MRD.UOMLevel = 2)
			            THEN
				            (SELECT Decimals FROM IC_UOM WHERE UOMID = I.UOMID2)
			            WHEN (MRD.UOMLevel = 3)
			            THEN
				            (SELECT Decimals FROM IC_UOM WHERE UOMID = I.UOMID3)
			            WHEN (MRD.UOMLevel = 4)
			            THEN
				            (SELECT Decimals FROM IC_UOM WHERE UOMID = I.UOMID4)
			            ELSE
				            0
			            END
		            )  AS UOMDecimals,
		            (
			            CASE WHEN (MRD.UOMLevel = 1) 
			            THEN 
				            (SELECT UOMID FROM IC_UOM WHERE UOMID = I.UOMID1)
			            WHEN (MRD.UOMLevel = 2)
			            THEN
				            (SELECT UOMID FROM IC_UOM WHERE UOMID = I.UOMID2)
			            WHEN (MRD.UOMLevel = 3)
			            THEN
				            (SELECT UOMID FROM IC_UOM WHERE UOMID = I.UOMID3)
			            WHEN (MRD.UOMLevel = 4)
			            THEN
				            (SELECT UOMID FROM IC_UOM WHERE UOMID = I.UOMID4)
			            ELSE
				            0
			            END
		            )  AS UOMIDx

            FROM AS_{Company}.dbo.AP_MR AS MR
             LEFT JOIN AS_{Company}.dbo.AP_MRDetails AS MRD ON MR.MRID = MRD.MRID
             LEFT JOIN AS_{Company}.dbo.IC_Items AS I ON MRD.ItemID = I.ItemID
            ";
        }
        public static string QueryOustandingAutoMR(string Company)
        {
            return @$"
            SELECT '{Company}' as Company,
                    (SELECT TOP 1 InventoryUserName FROM API.Ascend.Vessel x WHERE (SELECT TOP 1 RequiredByID FROM AS_{Company}.AUTO.AP_MRDetails WHERE MRID = MR.MRID) = x.InventoryUserID AND x.CompanyCode = '{Company}') AS Vessel,
                    (SELECT TOP 1 Fleet FROM API.Ascend.Vessel x WHERE (SELECT TOP 1 RequiredByID FROM AS_{Company}.AUTO.AP_MRDetails WHERE MRID = MR.MRID) = x.InventoryUserID AND x.CompanyCode = '{Company}') AS Fleet,
                    MR.MRID AS MRID,
                    MR.MRNumber AS MRNumber,
                    MR.MRCounter AS MRCounter,
                    MR.Class,
                    MR.Date,
                    MR.InventoryUserId AS InventoryUserID,
                    MR.Importance,
                    MR.RequiredDate,
                    MR.Void,
                    MR.VoidDateTime,
                    MR.VoidBy,
                    MR.VoidReason,
                    MR.Remarks AS MRHeader_Remarks,
                    MR.CreatedBy,
                    MR.CreateDate,
                    MR.LastModBy,
                    MR.LastMod,
                    MR.MRType,
                    MR.Approved,
                    MR.Purpose AS MRHeader_Purpose,
                    MR.ExternalRef,
                    MR.MR1stRequired AS MR1stRequired,
                    MR.MR1stApprovedBy AS MR1stApprovedBy,
                    MR.MR1stApprovedDateTime AS MR1stApprovedDateTime,
                    MR.MR2ndRequired AS MR2ndRequired,
                    MR.MR2ndApprovedBy AS MR2ndApprovedBy,
                    MR.MR2ndApprovedDateTime AS MR2ndApprovedDateTime,
                    MR.MR3rdRequired AS MR3rdRequired,
                    MR.MR3rdApprovedBy AS MR3rdApprovedBy,
                    MR.MR3rdApprovedDateTime AS MR3rdApprovedDateTime,
                    MR.MR4thRequired AS MR4thRequired,
                    MR.MR4thApprovedBy AS MR4thApprovedBy,
                    MR.MR4thApprovedDateTime AS MR4thApprovedDateTime,
                    MR.ProjectID,
                    MR.Closed,
                    MR.ClosedBy,
                    MR.ClosedDateTime,
                    MR.ClosedReason,
                    MR.IsReplacement,
                    MR.IsDraft,
                    MR.IsService,
                    MR.ServiceDeptID AS ServiceDeptID,
                    MR.AssetID AS AssetID,
                    MR.Problem,
                    MR.KMHM,
                    MR.ServiceAction,
                    MR.CompletedBy,
                    MR.CompletedDateTime,
                    MR.CompletedComment,
                    MR.UatBy,
                    MR.UatDateTime,
                    MR.UatComment,
                    MR.SubmitDateTime,
                    MR.DelegateToMRType AS DelegateToMRType,
                    MR.MCostAs,
                    MR.RefDate,
                    MR.SPWarehouseID AS SPWarehouseID,
                    MR.ExSource,
                    MR.ExSourceID AS ExSourceID,
                    MR.ForceClose,
                    MR.SvcMode,
                    MR.SvcItemID AS SvcItemID,
                    MR.SvcOther,
                    MR.Mode,
                    MR.MRServiceDeptID AS MRServiceDeptID,
                    MR.PLV2ItemID AS PLV2ItemID,
                    MR.Score,
                    MR.ForBAK,
                    MR.SvcPic,

                    MRD.MRDetailID,
                    MRD.MRID AS MRDetail_MRID,
                    MRD.ItemID AS MRDetail_ItemID,
                    MRD.UOMLevel,
                    MRD.Quantity,
                    MRD.Remarks AS MRDetail_Remarks,
                    MRD.RequiredByID,
                    MRD.GlobalQuantity,
                    MRD.GlobalUOMLevel,
                    MRD.Purpose AS MRDetail_Purpose,
                    MRD.DisplayIndex,
                    MRD.ItemDescription,
                    MRD.UOMID,
                    MRD.ItemRequiredDate,
                    MRD.DetailGuid,
                    MRD.DetailVoid,
                    MRD.DetailVoidBy,
                    MRD.DetailVoidDateTime,
                    MRD.DetailVoidReason,
                    MRD.WarehouseQuantity,
                    MRD.Comment,
                    MRD.DetailClosed,
                    MRD.DetailClosedBy,
                    MRD.DetailCloseReason,
                    MRD.DetailCostCenter,
                    MRD.QuantityRequestedExtra,
                    MRD.Booking,
                    MRD.Category,
                    MRD.BQDetailID,
                    MRD.RsvWarehouseID,
                    MRD.CostAs,
                    MRD.OldItemID,
                    MRD.DetailDelegateToMRType,
                    MRD.DetailDelegateReason,
                    MRD.AllowOverBQ,
                    MRD.AllowPRInc,
                    MRD.BookedDate,
                    MRD.DJobID,

                    I.ItemID AS ItemID,
                    I.ItemCode,
                    I.ItemName,
                    I.Specification,
                    I.PartNoEx,
                    I.UOMID1,
                    I.UOMID2,
                    I.UOMID3,
                    I.UOMID4,
		             (
			            CASE WHEN (MRD.UOMLevel = 1) 
			            THEN 
				            (SELECT UOMCode FROM IC_UOM WHERE UOMID = I.UOMID1)
			            WHEN (MRD.UOMLevel = 2)
			            THEN
				            (SELECT UOMCode FROM IC_UOM WHERE UOMID = I.UOMID2)
			            WHEN (MRD.UOMLevel = 3)
			            THEN
				            (SELECT UOMCode FROM IC_UOM WHERE UOMID = I.UOMID3)
			            WHEN (MRD.UOMLevel = 4)
			            THEN
				            (SELECT UOMCode FROM IC_UOM WHERE UOMID = I.UOMID4)
			            ELSE
				            ''
			            END
		            )  AS UOMCode,
		            (
			            CASE WHEN (MRD.UOMLevel = 1) 
			            THEN 
				            (SELECT Decimals FROM IC_UOM WHERE UOMID = I.UOMID1)
			            WHEN (MRD.UOMLevel = 2)
			            THEN
				            (SELECT Decimals FROM IC_UOM WHERE UOMID = I.UOMID2)
			            WHEN (MRD.UOMLevel = 3)
			            THEN
				            (SELECT Decimals FROM IC_UOM WHERE UOMID = I.UOMID3)
			            WHEN (MRD.UOMLevel = 4)
			            THEN
				            (SELECT Decimals FROM IC_UOM WHERE UOMID = I.UOMID4)
			            ELSE
				            0
			            END
		            )  AS UOMDecimals,
		            (
			            CASE WHEN (MRD.UOMLevel = 1) 
			            THEN 
				            (SELECT UOMID FROM IC_UOM WHERE UOMID = I.UOMID1)
			            WHEN (MRD.UOMLevel = 2)
			            THEN
				            (SELECT UOMID FROM IC_UOM WHERE UOMID = I.UOMID2)
			            WHEN (MRD.UOMLevel = 3)
			            THEN
				            (SELECT UOMID FROM IC_UOM WHERE UOMID = I.UOMID3)
			            WHEN (MRD.UOMLevel = 4)
			            THEN
				            (SELECT UOMID FROM IC_UOM WHERE UOMID = I.UOMID4)
			            ELSE
				            0
			            END
		            )  AS UOMIDx

            FROM AS_{Company}.AUTO.AP_MR AS MR
             LEFT JOIN AS_{Company}.AUTO.AP_MRDetails AS MRD ON MR.MRID = MRD.MRID
             LEFT JOIN AS_{Company}.dbo.IC_Items AS I ON MRD.ItemID = I.ItemID
            ";
        }
        public static string QueryMR2(string Company, string schema = "dbo")
        {

            return $@"'{Company}' as Company,
                    (SELECT TOP 1 InventoryUserName FROM API.Ascend.Vessel x WHERE (SELECT TOP 1 RequiredByID FROM AS_{Company}.{schema}.AP_MRDetails WHERE MRID = MR.MRID) = x.InventoryUserID AND x.CompanyCode = '{Company}') AS Vessel,
                    (SELECT TOP 1 Fleet FROM API.Ascend.Vessel x WHERE (SELECT TOP 1 RequiredByID FROM AS_{Company}.{schema}.AP_MRDetails WHERE MRID = MR.MRID) = x.InventoryUserID AND x.CompanyCode = '{Company}') AS Fleet,
                    MR.MRID AS MRID,
                    MR.MRNumber AS MRNumber,
                    MR.MRCounter AS MRCounter,
                    MR.Class,
                    MR.Date,
                    MR.InventoryUserId AS InventoryUserID,
                    MR.Importance,
                    MR.RequiredDate,
                    MR.Void,
                    MR.VoidDateTime,
                    MR.VoidBy,
                    MR.VoidReason,
                    MR.Remarks AS MRHeader_Remarks,
                    MR.CreatedBy,
                    MR.CreateDate,
                    MR.LastModBy,
                    MR.LastMod,
                    MR.MRType,
                    MR.Approved,
                    MR.Purpose AS MRHeader_Purpose,
                    MR.ExternalRef,
                    MR.MR1stRequired AS MR1stRequired,
                    MR.MR1stApprovedBy AS MR1stApprovedBy,
                    MR.MR1stApprovedDateTime AS MR1stApprovedDateTime,
                    MR.MR2ndRequired AS MR2ndRequired,
                    MR.MR2ndApprovedBy AS MR2ndApprovedBy,
                    MR.MR2ndApprovedDateTime AS MR2ndApprovedDateTime,
                    MR.MR3rdRequired AS MR3rdRequired,
                    MR.MR3rdApprovedBy AS MR3rdApprovedBy,
                    MR.MR3rdApprovedDateTime AS MR3rdApprovedDateTime,
                    MR.MR4thRequired AS MR4thRequired,
                    MR.MR4thApprovedBy AS MR4thApprovedBy,
                    MR.MR4thApprovedDateTime AS MR4thApprovedDateTime,
                    MR.ProjectID,
                    MR.Closed,
                    MR.ClosedBy,
                    MR.ClosedDateTime,
                    MR.ClosedReason,
                    MR.IsReplacement,
                    MR.IsDraft,
                    MR.IsService,
                    MR.ServiceDeptID AS ServiceDeptID,
                    MR.AssetID AS AssetID,
                    MR.Problem,
                    MR.KMHM,
                    MR.ServiceAction,
                    MR.CompletedBy,
                    MR.CompletedDateTime,
                    MR.CompletedComment,
                    MR.UatBy,
                    MR.UatDateTime,
                    MR.UatComment,
                    MR.SubmitDateTime,
                    MR.DelegateToMRType AS DelegateToMRType,
                    MR.MCostAs,
                    MR.RefDate,
                    MR.SPWarehouseID AS SPWarehouseID,
                    MR.ExSource,
                    MR.ExSourceID AS ExSourceID,
                    MR.ForceClose,
                    MR.SvcMode,
                    MR.SvcItemID AS SvcItemID,
                    MR.SvcOther,
                    MR.Mode,
                    MR.MRServiceDeptID AS MRServiceDeptID,
                    MR.PLV2ItemID AS PLV2ItemID,
                    MR.Score,
                    MR.ForBAK,
                    MR.SvcPic,

                    MRD.MRDetailID,
                    MRD.MRID AS MRDetail_MRID,
                    MRD.ItemID AS MRDetail_ItemID,
                    MRD.UOMLevel,
                    MRD.Quantity,
                    MRD.Remarks AS MRDetail_Remarks,
                    MRD.RequiredByID,
                    MRD.GlobalQuantity,
                    MRD.GlobalUOMLevel,
                    MRD.Purpose AS MRDetail_Purpose,
                    MRD.DisplayIndex,
                    MRD.ItemDescription,
                    MRD.UOMID,
                    MRD.ItemRequiredDate,
                    MRD.DetailGuid,
                    MRD.DetailVoid,
                    MRD.DetailVoidBy,
                    MRD.DetailVoidDateTime,
                    MRD.DetailVoidReason,
                    MRD.WarehouseQuantity,
                    MRD.Comment,
                    MRD.DetailClosed,
                    MRD.DetailClosedBy,
                    MRD.DetailCloseReason,
                    MRD.DetailCostCenter,
                    MRD.QuantityRequestedExtra,
                    MRD.Booking,
                    MRD.Category,
                    MRD.BQDetailID,
                    MRD.RsvWarehouseID,
                    MRD.CostAs,
                    MRD.OldItemID,
                    MRD.DetailDelegateToMRType,
                    MRD.DetailDelegateReason,
                    MRD.AllowOverBQ,
                    MRD.AllowPRInc,
                    MRD.BookedDate,
                    MRD.DJobID,

                    I.ItemID AS ItemID,
                    I.ItemCode,
                    I.ItemName,
                    I.Specification,
                    I.PartNoEx,
                    I.UOMID1,
                    I.UOMID2,
                    I.UOMID3,
                    I.UOMID4,
		             (
			            CASE WHEN (MRD.UOMLevel = 1) 
			            THEN 
				            (SELECT UOMCode FROM IC_UOM WHERE UOMID = I.UOMID1)
			            WHEN (MRD.UOMLevel = 2)
			            THEN
				            (SELECT UOMCode FROM IC_UOM WHERE UOMID = I.UOMID2)
			            WHEN (MRD.UOMLevel = 3)
			            THEN
				            (SELECT UOMCode FROM IC_UOM WHERE UOMID = I.UOMID3)
			            WHEN (MRD.UOMLevel = 4)
			            THEN
				            (SELECT UOMCode FROM IC_UOM WHERE UOMID = I.UOMID4)
			            ELSE
				            ''
			            END
		            )  AS UOMCode,
		            (
			            CASE WHEN (MRD.UOMLevel = 1) 
			            THEN 
				            (SELECT Decimals FROM IC_UOM WHERE UOMID = I.UOMID1)
			            WHEN (MRD.UOMLevel = 2)
			            THEN
				            (SELECT Decimals FROM IC_UOM WHERE UOMID = I.UOMID2)
			            WHEN (MRD.UOMLevel = 3)
			            THEN
				            (SELECT Decimals FROM IC_UOM WHERE UOMID = I.UOMID3)
			            WHEN (MRD.UOMLevel = 4)
			            THEN
				            (SELECT Decimals FROM IC_UOM WHERE UOMID = I.UOMID4)
			            ELSE
				            0
			            END
		            )  AS UOMDecimals,
		            (
			            CASE WHEN (MRD.UOMLevel = 1) 
			            THEN 
				            (SELECT UOMID FROM IC_UOM WHERE UOMID = I.UOMID1)
			            WHEN (MRD.UOMLevel = 2)
			            THEN
				            (SELECT UOMID FROM IC_UOM WHERE UOMID = I.UOMID2)
			            WHEN (MRD.UOMLevel = 3)
			            THEN
				            (SELECT UOMID FROM IC_UOM WHERE UOMID = I.UOMID3)
			            WHEN (MRD.UOMLevel = 4)
			            THEN
				            (SELECT UOMID FROM IC_UOM WHERE UOMID = I.UOMID4)
			            ELSE
				            0
			            END
		            )  AS UOMIDx";
        }
        public static string QueryMR(string Company, string schema = "dbo")
        {
            var Where = "";
            if (schema == "dbo")
            {
                //WHERE MR.Approved = 0 AND MR.Closed = 0 AND MR.Void = 0
                Where = "WHERE MR.Approved = 1  AND MR.Void = 0";
            }
            else
            {
                Where = "WHERE MR.MRNumber <> ''";
            }
            return $@"
                   SELECT '{Company}' as Company,
                    V.InventoryUserName AS Vessel,
                    V.Fleet,
                    MR.MRID AS MRID,
                    MR.MRNumber,
                    MR.Date,
                    MR.InventoryUserID,
                    MR.Importance,
                    MR.RequiredDate,
                    MR.Void,
                    MR.VoidDateTime,
                    MR.VoidBy,
                    MR.VoidReason,
                    MR.Remarks AS MRHeader_Remarks,
                    MR.CreatedBy,
                    MR.CreateDate,
                    MR.LastModBy,
                    MR.LastMod,
                    MR.MRType,
                    MR.Approved,
                    MR.ExternalRef,
                    MR.MR1stRequired AS MR1stRequired,
                    MR.MR1stApprovedBy AS MR1stApprovedBy,
                    MR.MR1stApprovedDateTime AS MR1stApprovedDateTime,
                    MR.MR2ndRequired AS MR2ndRequired,
                    MR.MR2ndApprovedBy AS MR2ndApprovedBy,
                    MR.MR2ndApprovedDateTime AS MR2ndApprovedDateTime,
                    MR.MR3rdRequired AS MR3rdRequired,
                    MR.MR3rdApprovedBy AS MR3rdApprovedBy,
                    MR.MR3rdApprovedDateTime AS MR3rdApprovedDateTime,
                    MR.MR4thRequired AS MR4thRequired,
                    MR.MR4thApprovedBy AS MR4thApprovedBy,
                    MR.MR4thApprovedDateTime AS MR4thApprovedDateTime,
                    MR.Closed,
                    MR.ClosedBy,
                    MR.ClosedDateTime,
                    MR.ClosedReason,
                    MR.IsReplacement,
                    MR.IsDraft,
                    MR.IsService,
                    MR.ServiceDeptID,
                    MR.AssetID,
                    MR.Problem,
                    MR.KMHM,
                    MR.ServiceAction,
                    MR.SubmitDateTime,
                    MR.DelegateToMRType,
                    MR.MCostAs,
                    MR.RefDate,
                    MR.SPWarehouseID,
                    MR.ExSource,
                    MR.ExSourceID,
                    MR.ForceClose,
                    MR.SvcMode,
                    MR.SvcItemID,
                    MR.SvcOther,
                    MR.Mode,
                    MR.MRServiceDeptID,
                    MR.PLV2ItemID,
                    MR.Score,
                    MR.ForBAK,
                    MR.SvcPic,

                    MRD.MRDetailID,
                    MRD.MRID AS MRDetail_MRID,
                    MRD.ItemID AS MRDetail_ItemID,
                    MRD.UOMLevel,
                    MRD.Quantity,
                    MRD.Remarks AS MRDetail_Remarks,
                    MRD.RequiredByID,
                    MRD.GlobalQuantity,
                    MRD.GlobalUOMLevel,
                    MRD.Purpose AS MRDetail_Purpose,
                    MRD.DisplayIndex,
                    MRD.ItemDescription,
                    MRD.UOMID,
                    MRD.ItemRequiredDate,
                    MRD.DetailGuid,
                    MRD.DetailVoid,
                    MRD.DetailVoidBy,
                    MRD.DetailVoidDateTime,
                    MRD.DetailVoidReason,
                    MRD.WarehouseQuantity,
                    MRD.Comment,
                    MRD.DetailClosed,
                    MRD.DetailClosedBy,
                    MRD.DetailCloseReason,
                    MRD.DetailCostCenter,
                    MRD.QuantityRequestedExtra,
                    MRD.Booking,
                    MRD.Category,
                    MRD.RsvWarehouseID,
                    MRD.CostAs,
                    MRD.OldItemID,
                    MRD.DetailDelegateToMRType,
                    MRD.DetailDelegateReason,
                    MRD.AllowOverBQ,
                    MRD.AllowPRInc,
                    MRD.BookedDate,
                    MRD.DJobID,
		              (
			            CASE WHEN (MRD.UOMLevel = 1) 
			            THEN 
				            UOM1.UOMCode
			            WHEN (MRD.UOMLevel = 2)
			            THEN
				            UOM2.UOMCode
			            WHEN (MRD.UOMLevel = 3)
			            THEN
				            UOM3.UOMCode
			            WHEN (MRD.UOMLevel = 4)
			            THEN
				            UOM4.UOMCode
			            ELSE
				            ''
			            END
		            )  AS UOMCode,
		            (
			            CASE WHEN (MRD.UOMLevel = 1) 
			            THEN 
				            UOM1.Decimals
			            WHEN (MRD.UOMLevel = 2)
			            THEN
				            UOM2.Decimals
			            WHEN (MRD.UOMLevel = 3)
			            THEN
				            UOM3.Decimals
			            WHEN (MRD.UOMLevel = 4)
			            THEN
				            UOM4.Decimals
			            ELSE
				            0
			            END
		            )  AS UOMDecimals,
		            (
			            CASE WHEN (MRD.UOMLevel = 1) 
			            THEN 
				            UOM1.UOMID
			            WHEN (MRD.UOMLevel = 2)
			            THEN
				            UOM2.UOMID
			            WHEN (MRD.UOMLevel = 3)
			            THEN
				            UOM3.UOMID
			            WHEN (MRD.UOMLevel = 4)
			            THEN
				            UOM4.UOMID
			            ELSE
				            0
			            END
		            )  AS UOMIDx
                FROM AS_{Company}.{schema}.AP_MR AS MR
                LEFT JOIN AS_{Company}.dbo.AP_MRDetails AS MRD ON MR.MRID = MRD.MRID
	            LEFT JOIN API.Ascend.Vessel V ON MRD.RequiredByID = V.InventoryUserID 
                LEFT JOIN AS_{Company}.dbo.IC_Items AS I ON MRD.ItemID = I.ItemID
                LEFT JOIN AS_WNS.dbo.IC_UOM AS UOM1 ON UOM1.UOMID = I.UOMID1
				LEFT JOIN AS_WNS.dbo.IC_UOM AS UOM2 ON UOM2.UOMID = I.UOMID2
				LEFT JOIN AS_WNS.dbo.IC_UOM AS UOM3 ON UOM3.UOMID = I.UOMID3
				LEFT JOIN AS_WNS.dbo.IC_UOM AS UOM4 ON UOM4.UOMID = I.UOMID4
                {Where}
            ";
        }
        public static string QueryMR1(string Company, string schema = "dbo")
        {
            var Where = "";
            if (schema == "dbo")
            {
                //WHERE MR.Approved = 0 AND MR.Closed = 0 AND MR.Void = 0
                Where = "WHERE MR.Approved = 1  AND MR.Void = 0";
            }
            else
            {
                Where = "WHERE MR.MRNumber <> ''";
            }
            return $@"
                   SELECT '{Company}' as Company,
                    (SELECT TOP 1 InventoryUserName FROM API.Ascend.Vessel x WHERE (SELECT TOP 1 RequiredByID FROM AS_{Company}.{schema}.AP_MRDetails WHERE MRID = MR.MRID) = x.InventoryUserID AND x.CompanyCode = '{Company}') AS Vessel,
                    (SELECT TOP 1 Fleet FROM API.Ascend.Vessel x WHERE (SELECT TOP 1 RequiredByID FROM AS_{Company}.{schema}.AP_MRDetails WHERE MRID = MR.MRID) = x.InventoryUserID AND x.CompanyCode = '{Company}') AS Fleet,
                    MR.MRID AS MRID,
                    MR.MRNumber AS MRNumber,
                    MR.MRCounter AS MRCounter,
                    MR.Class,
                    MR.Date,
                    MR.InventoryUserId AS InventoryUserID,
                    MR.Importance,
                    MR.RequiredDate,
                    MR.Void,
                    MR.VoidDateTime,
                    MR.VoidBy,
                    MR.VoidReason,
                    MR.Remarks AS MRHeader_Remarks,
                    MR.CreatedBy,
                    MR.CreateDate,
                    MR.LastModBy,
                    MR.LastMod,
                    MR.MRType,
                    MR.Approved,
                    MR.ExternalRef,
                    MR.MR1stRequired AS MR1stRequired,
                    MR.MR1stApprovedBy AS MR1stApprovedBy,
                    MR.MR1stApprovedDateTime AS MR1stApprovedDateTime,
                    MR.MR2ndRequired AS MR2ndRequired,
                    MR.MR2ndApprovedBy AS MR2ndApprovedBy,
                    MR.MR2ndApprovedDateTime AS MR2ndApprovedDateTime,
                    MR.MR3rdRequired AS MR3rdRequired,
                    MR.MR3rdApprovedBy AS MR3rdApprovedBy,
                    MR.MR3rdApprovedDateTime AS MR3rdApprovedDateTime,
                    MR.MR4thRequired AS MR4thRequired,
                    MR.MR4thApprovedBy AS MR4thApprovedBy,
                    MR.MR4thApprovedDateTime AS MR4thApprovedDateTime,
                    MR.ProjectID,
                    MR.Closed,
                    MR.ClosedBy,
                    MR.ClosedDateTime,
                    MR.ClosedReason,
                    MR.IsReplacement,
                    MR.IsDraft,
                    MR.IsService,
                    MR.ServiceDeptID AS ServiceDeptID,
                    MR.AssetID AS AssetID,
                    MR.Problem,
                    MR.KMHM,
                    MR.ServiceAction,
                    MR.CompletedBy,
                    MR.CompletedDateTime,
                    MR.CompletedComment,
                    MR.UatBy,
                    MR.UatDateTime,
                    MR.UatComment,
                    MR.SubmitDateTime,
                    MR.DelegateToMRType AS DelegateToMRType,
                    MR.MCostAs,
                    MR.RefDate,
                    MR.SPWarehouseID AS SPWarehouseID,
                    MR.ExSource,
                    MR.ExSourceID AS ExSourceID,
                    MR.ForceClose,
                    MR.SvcMode,
                    MR.SvcItemID AS SvcItemID,
                    MR.SvcOther,
                    MR.Mode,
                    MR.MRServiceDeptID AS MRServiceDeptID,
                    MR.PLV2ItemID AS PLV2ItemID,
                    MR.Score,
                    MR.ForBAK,
                    MR.SvcPic,

                    MRD.MRDetailID,
                    MRD.MRID AS MRDetail_MRID,
                    MRD.ItemID AS MRDetail_ItemID,
                    MRD.UOMLevel,
                    MRD.Quantity,
                    MRD.Remarks AS MRDetail_Remarks,
                    MRD.RequiredByID,
                    MRD.GlobalQuantity,
                    MRD.GlobalUOMLevel,
                    MRD.Purpose AS MRDetail_Purpose,
                    MRD.DisplayIndex,
                    MRD.ItemDescription,
                    MRD.UOMID,
                    MRD.ItemRequiredDate,
                    MRD.DetailGuid,
                    MRD.DetailVoid,
                    MRD.DetailVoidBy,
                    MRD.DetailVoidDateTime,
                    MRD.DetailVoidReason,
                    MRD.WarehouseQuantity,
                    MRD.Comment,
                    MRD.DetailClosed,
                    MRD.DetailClosedBy,
                    MRD.DetailCloseReason,
                    MRD.DetailCostCenter,
                    MRD.QuantityRequestedExtra,
                    MRD.Booking,
                    MRD.Category,
                    MRD.BQDetailID,
                    MRD.RsvWarehouseID,
                    MRD.CostAs,
                    MRD.OldItemID,
                    MRD.DetailDelegateToMRType,
                    MRD.DetailDelegateReason,
                    MRD.AllowOverBQ,
                    MRD.AllowPRInc,
                    MRD.BookedDate,
                    MRD.DJobID,

                    I.ItemID AS ItemID,
                    I.ItemCode,
                    I.ItemName,
                    I.Specification,
                    I.PartNoEx,
                    I.UOMID1,
                    I.UOMID2,
                    I.UOMID3,
                    I.UOMID4,
		             (
			            CASE WHEN (MRD.UOMLevel = 1) 
			            THEN 
				            (SELECT UOMCode FROM IC_UOM WHERE UOMID = I.UOMID1)
			            WHEN (MRD.UOMLevel = 2)
			            THEN
				            (SELECT UOMCode FROM IC_UOM WHERE UOMID = I.UOMID2)
			            WHEN (MRD.UOMLevel = 3)
			            THEN
				            (SELECT UOMCode FROM IC_UOM WHERE UOMID = I.UOMID3)
			            WHEN (MRD.UOMLevel = 4)
			            THEN
				            (SELECT UOMCode FROM IC_UOM WHERE UOMID = I.UOMID4)
			            ELSE
				            ''
			            END
		            )  AS UOMCode,
		            (
			            CASE WHEN (MRD.UOMLevel = 1) 
			            THEN 
				            (SELECT Decimals FROM IC_UOM WHERE UOMID = I.UOMID1)
			            WHEN (MRD.UOMLevel = 2)
			            THEN
				            (SELECT Decimals FROM IC_UOM WHERE UOMID = I.UOMID2)
			            WHEN (MRD.UOMLevel = 3)
			            THEN
				            (SELECT Decimals FROM IC_UOM WHERE UOMID = I.UOMID3)
			            WHEN (MRD.UOMLevel = 4)
			            THEN
				            (SELECT Decimals FROM IC_UOM WHERE UOMID = I.UOMID4)
			            ELSE
				            0
			            END
		            )  AS UOMDecimals,
		            (
			            CASE WHEN (MRD.UOMLevel = 1) 
			            THEN 
				            (SELECT UOMID FROM IC_UOM WHERE UOMID = I.UOMID1)
			            WHEN (MRD.UOMLevel = 2)
			            THEN
				            (SELECT UOMID FROM IC_UOM WHERE UOMID = I.UOMID2)
			            WHEN (MRD.UOMLevel = 3)
			            THEN
				            (SELECT UOMID FROM IC_UOM WHERE UOMID = I.UOMID3)
			            WHEN (MRD.UOMLevel = 4)
			            THEN
				            (SELECT UOMID FROM IC_UOM WHERE UOMID = I.UOMID4)
			            ELSE
				            0
			            END
		            )  AS UOMIDx
                FROM AS_{Company}.{schema}.AP_MR AS MR
                LEFT JOIN AS_{Company}.dbo.AP_MRDetails AS MRD ON MR.MRID = MRD.MRID
                LEFT JOIN AS_{Company}.dbo.IC_Items AS I ON MRD.ItemID = I.ItemID
                {Where}
            ";
        }
    }
}
