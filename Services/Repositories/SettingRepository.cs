using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Restaurant.Models;

namespace Restaurant.Services.Repositories
{
    public class SettingRepository : IRepository<Setting>
    {
        private readonly DatabaseConnectionService _dbService;

        public SettingRepository(DatabaseConnectionService dbService)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
        }

        public IEnumerable<Setting> GetAll()
        {
            List<Setting> settings = new List<Setting>();

            DataTable dataTable = _dbService.ExecuteReader("usp_GetAllSettings");

            foreach (DataRow row in dataTable.Rows)
            {
                settings.Add(MapRowToSetting(row));
            }

            return settings;
        }

        public Setting GetById(int id)
        {
            SqlParameter parameter = _dbService.CreateParameter("@SettingId", SqlDbType.Int, id);

            DataTable dataTable = _dbService.ExecuteReader("usp_GetSettingById", parameter);

            if (dataTable.Rows.Count > 0)
            {
                return MapRowToSetting(dataTable.Rows[0]);
            }

            return null;
        }

        public Setting GetByKey(string key)
        {
            SqlParameter parameter = _dbService.CreateParameter("@Key", SqlDbType.NVarChar, key);

            DataTable dataTable = _dbService.ExecuteReader("usp_GetSettingByKey", parameter);

            if (dataTable.Rows.Count > 0)
            {
                return MapRowToSetting(dataTable.Rows[0]);
            }

            return null;
        }

        public int Add(Setting setting)
        {
            SqlParameter[] parameters =
            {
                _dbService.CreateParameter("@Key", SqlDbType.NVarChar, setting.Key),
                _dbService.CreateParameter("@Value", SqlDbType.NVarChar, setting.Value),
                _dbService.CreateParameter("@Description", SqlDbType.NVarChar, setting.Description),
                _dbService.CreateParameter("@SettingId", SqlDbType.Int, null)
            };

            parameters[3].Direction = ParameterDirection.Output;

            _dbService.ExecuteNonQuery("usp_AddSetting", parameters);

            return (int)parameters[3].Value;
        }

        public bool Update(Setting setting)
        {
            SqlParameter[] parameters =
            {
                _dbService.CreateParameter("@SettingId", SqlDbType.Int, setting.SettingId),
                _dbService.CreateParameter("@Key", SqlDbType.NVarChar, setting.Key),
                _dbService.CreateParameter("@Value", SqlDbType.NVarChar, setting.Value),
                _dbService.CreateParameter("@Description", SqlDbType.NVarChar, setting.Description)
            };

            int rowsAffected = _dbService.ExecuteNonQuery("usp_UpdateSetting", parameters);

            return rowsAffected > 0;
        }

        public bool UpdateValue(string key, string value)
        {
            SqlParameter[] parameters =
            {
                _dbService.CreateParameter("@Key", SqlDbType.NVarChar, key),
                _dbService.CreateParameter("@Value", SqlDbType.NVarChar, value)
            };

            int rowsAffected = _dbService.ExecuteNonQuery("usp_UpdateSettingValue", parameters);

            return rowsAffected > 0;
        }

        public bool Delete(int id)
        {
            SqlParameter parameter = _dbService.CreateParameter("@SettingId", SqlDbType.Int, id);

            int rowsAffected = _dbService.ExecuteNonQuery("usp_DeleteSetting", parameter);

            return rowsAffected > 0;
        }

        public decimal GetDecimalValue(string key, decimal defaultValue = 0)
        {
            Setting setting = GetByKey(key);

            if (setting != null && !string.IsNullOrEmpty(setting.Value))
            {
                if (decimal.TryParse(setting.Value, out decimal value))
                {
                    return value;
                }
            }

            return defaultValue;
        }

        public int GetIntValue(string key, int defaultValue = 0)
        {
            Setting setting = GetByKey(key);

            if (setting != null && !string.IsNullOrEmpty(setting.Value))
            {
                if (int.TryParse(setting.Value, out int value))
                {
                    return value;
                }
            }

            return defaultValue;
        }

        public bool GetBoolValue(string key, bool defaultValue = false)
        {
            Setting setting = GetByKey(key);

            if (setting != null && !string.IsNullOrEmpty(setting.Value))
            {
                if (bool.TryParse(setting.Value, out bool value))
                {
                    return value;
                }
            }

            return defaultValue;
        }

        private Setting MapRowToSetting(DataRow row)
        {
            return new Setting
            {
                SettingId = Convert.ToInt32(row["SettingId"]),
                Key = row["Key"].ToString(),
                Value = row["Value"].ToString(),
                Description = row["Description"] == DBNull.Value ? null : row["Description"].ToString()
            };
        }
    }
}