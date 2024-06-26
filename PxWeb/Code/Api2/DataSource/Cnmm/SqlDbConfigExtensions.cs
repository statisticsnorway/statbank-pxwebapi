﻿using System.Data;
using System.Linq;

using Microsoft.Extensions.Options;

using PCAxis.Menu;
using PCAxis.Sql;
using PCAxis.Sql.DbClient;
using PCAxis.Sql.DbConfig;

namespace PxWeb.Code.Api2.DataSource.Cnmm
{
    public static class SqlDbConfigExtensions
    {
        public static Dictionary<string, ItemSelection>? GetMenuLookupTables(this SqlDbConfig DB, string language, IOptions<PxApiConfigurationOptions> configOptions)
        {
            return GetMenuLookup(DB, language, false, configOptions);
        }
        public static Dictionary<string, ItemSelection>? GetMenuLookupFolders(this SqlDbConfig DB, string language, IOptions<PxApiConfigurationOptions> configOptions)
        {
            return GetMenuLookup(DB, language, true, configOptions);
        }


        private static Dictionary<string, ItemSelection>? GetMenuLookup(this SqlDbConfig DB, string language, bool folders, IOptions<PxApiConfigurationOptions> configOptions)
        {
            // Check language to avoid SQL injection
            if (!configOptions.Value.Languages.Any(l => l.Id == language))
            {
                throw new ArgumentException($"Illegal language {language}");
            }

            var menuLookup = new Dictionary<string, ItemSelection>();

            string sql;
            if (DB is SqlDbConfig_21 sqlDbConfig_21)
            {
                if (folders)
                {
                    sql = GetMenuLookupFoldersQuery2_1(sqlDbConfig_21, language);
                }
                else
                {
                    sql = GetMenuLookupTablesQuery2_1(sqlDbConfig_21, language);
                }
            }
            else if (DB is SqlDbConfig_22 sqlDbConfig_22)
            {
                if (folders)
                {
                    sql = GetMenuLookupFoldersQuery2_2(sqlDbConfig_22, language);
                }
                else
                {
                    sql = GetMenuLookupTablesQuery2_2(sqlDbConfig_22, language);
                }
            }
            else if (DB is SqlDbConfig_23 sqlDbConfig_23)
            {
                if (folders)
                {
                    sql = GetMenuLookupFoldersQuery2_3(sqlDbConfig_23, language);
                }
                else
                {
                    sql = GetMenuLookupTablesQuery2_3(sqlDbConfig_23, language);
                }
            }
            else if (DB is SqlDbConfig_24 sqlDbConfig_24)
            {
                if (folders)
                {
                    sql = GetMenuLookupFoldersQuery2_4(sqlDbConfig_24, language);
                }
                else
                {
                    sql = GetMenuLookupTablesQuery2_4(sqlDbConfig_24, language);
                }
            }
            else
            {
                return null;
            }

            var cmd = GetPxSqlCommand(DB);

            var dataSet = cmd.ExecuteSelect(sql);
            ItemSelection itemSelection;

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                string key = row[2].ToString()?.ToUpper() ?? string.Empty;
                string? menu = row[0].ToString();
                string? selection = row[1].ToString();

                if (!menuLookup.ContainsKey(key))
                {
                    itemSelection = new ItemSelection(menu, selection);
                    menuLookup.Add(key, itemSelection); // Key always uppercase
                }
                else
                {
                    // TODO: Log that this is a duplicate key
                    Console.WriteLine(row[0] + " " + row[1]);
                }
            }

            if (folders && !menuLookup.ContainsKey("START"))
            {
                itemSelection = new ItemSelection("START", "START");
                menuLookup.Add("START", itemSelection);
            }

            return menuLookup;
        }

        private static string GetMenuLookupFoldersQuery2_1(SqlDbConfig_21 DB, string language)
        {
            if (!DB.isSecondaryLanguage(language))
            {
                return $@"SELECT 
                            {DB.MenuSelection.MenuCol.ForSelect()}, 
                            {DB.MenuSelection.SelectionCol.ForSelect()}, 
                            {DB.MenuSelection.SelectionCol.ForSelect()} 
                        FROM 
                            {DB.MenuSelection.GetNameAndAlias()}
                        WHERE 
                            {DB.MenuSelection.LevelNoCol.Id()} NOT IN (SELECT {DB.MetaAdm.ValueCol} FROM {DB.MetaAdm.GetNameAndAlias()} WHERE upper({DB.MetaAdm.PropertyCol.Id()}) = 'MENULEVELS')";
            }
            else
            {
                return $@"SELECT 
                            {DB.MenuSelectionLang2.MenuCol.ForSelect(language)}, 
                            {DB.MenuSelectionLang2.SelectionCol.ForSelect(language)}, 
                            {DB.MenuSelectionLang2.SelectionCol.ForSelect(language)}
                    FROM 
                            {DB.MenuSelectionLang2.GetNameAndAlias(language)} 
                        JOIN {DB.MenuSelectionLang2.GetNameAndAlias(language)} ON {DB.MenuSelectionLang2.MenuCol.Id(language)} = {DB.MenuSelection.MenuCol.Id()} AND {DB.MenuSelectionLang2.SelectionCol.Id(language)} = {DB.MenuSelection.SelectionCol.Id()}
                    WHERE 
                            {DB.MenuSelection.LevelNoCol.Id()} NOT IN (SELECT {DB.MetaAdm.ValueCol} FROM {DB.MetaAdm.GetNameAndAlias()} WHERE upper({DB.MetaAdm.PropertyCol.Id()}) = 'MENULEVELS')";
            }
        }

        private static string GetMenuLookupTablesQuery2_1(SqlDbConfig_21 DB, string language)
        {
            if (!DB.isSecondaryLanguage(language))
            {
                return $@"SELECT 
                            {DB.MenuSelection.MenuCol.ForSelect()}, 
                            {DB.MainTable.MainTableCol.ForSelect()}, 
                            {DB.MainTable.TableIdCol.ForSelect()} 
                        FROM 
                            {DB.MainTable.GetNameAndAlias()} 
                            JOIN {DB.MenuSelection.GetNameAndAlias()} ON {DB.MenuSelection.SelectionCol.Id()} = {DB.MainTable.MainTableCol.Id()}";
            }
            else
            {
                return $@"SELECT 
                            {DB.MenuSelection.MenuCol.ForSelect()}, 
                            {DB.MainTable.MainTableCol.ForSelect()}, 
                            {DB.MainTable.TableIdCol.ForSelect()} 
                    FROM 
                            {DB.MainTable.GetNameAndAlias()} 
                            JOIN {DB.MenuSelectionLang2.GetNameAndAlias(language)} ON {DB.MainTable.MainTableCol.Id()} = {DB.MainTableLang2.MainTableCol.Id(language)} 
                            JOIN {DB.MenuSelection.GetNameAndAlias()} ON {DB.MenuSelection.SelectionCol.Id()} = {DB.MainTable.MainTableCol.Id()}
                    WHERE 
                            {DB.MainTableLang2.StatusCol.Id(language)} = '{DB.Codes.Yes}'";
            }
        }


        private static string GetMenuLookupFoldersQuery2_2(this SqlDbConfig_22 DB, string language)
        {
            if (!DB.isSecondaryLanguage(language))
            {
                return $@"SELECT 
                            {DB.MenuSelection.MenuCol.ForSelect()}, 
                            {DB.MenuSelection.SelectionCol.ForSelect()}, 
                            {DB.MenuSelection.SelectionCol.ForSelect()} 
                        FROM 
                            {DB.MenuSelection.GetNameAndAlias()}
                        WHERE 
                            {DB.MenuSelection.LevelNoCol.Id()} NOT IN (SELECT {DB.MetaAdm.ValueCol} FROM {DB.MetaAdm.GetNameAndAlias()} WHERE upper({DB.MetaAdm.PropertyCol.Id()}) = 'MENULEVELS')";
            }
            else
            {
                return $@"SELECT 
                            {DB.MenuSelectionLang2.MenuCol.ForSelect(language)}, 
                            {DB.MenuSelectionLang2.SelectionCol.ForSelect(language)}, 
                            {DB.MenuSelectionLang2.SelectionCol.ForSelect(language)}
                    FROM 
                            {DB.MenuSelectionLang2.GetNameAndAlias(language)} 
                        JOIN {DB.MenuSelectionLang2.GetNameAndAlias(language)} ON {DB.MenuSelectionLang2.MenuCol.Id(language)} = {DB.MenuSelection.MenuCol.Id()} AND {DB.MenuSelectionLang2.SelectionCol.Id(language)} = {DB.MenuSelection.SelectionCol.Id()}
                    WHERE 
                            {DB.MenuSelection.LevelNoCol.Id()} NOT IN (SELECT {DB.MetaAdm.ValueCol} FROM {DB.MetaAdm.GetNameAndAlias()} WHERE upper({DB.MetaAdm.PropertyCol.Id()}) = 'MENULEVELS')";
            }
        }

        private static string GetMenuLookupTablesQuery2_2(this SqlDbConfig_22 DB, string language)
        {
            if (!DB.isSecondaryLanguage(language))
            {
                return $@"SELECT 
                            {DB.MenuSelection.MenuCol.ForSelect()}, 
                            {DB.MainTable.MainTableCol.ForSelect()}, 
                            {DB.MainTable.TableIdCol.ForSelect()} 
                        FROM 
                            {DB.MainTable.GetNameAndAlias()} 
                            JOIN {DB.MenuSelection.GetNameAndAlias()} ON {DB.MenuSelection.SelectionCol.Id()} = {DB.MainTable.MainTableCol.Id()}";
            }
            else
            {
                return $@"SELECT 
                            {DB.MenuSelection.MenuCol.ForSelect()}, 
                            {DB.MainTable.MainTableCol.ForSelect()}, 
                            {DB.MainTable.TableIdCol.ForSelect()} 
                    FROM 
                            {DB.MainTable.GetNameAndAlias()} 
                            JOIN {DB.MenuSelectionLang2.GetNameAndAlias(language)} ON {DB.MainTable.MainTableCol.Id()} = {DB.MainTableLang2.MainTableCol.Id(language)} 
                            JOIN {DB.MenuSelection.GetNameAndAlias()} ON {DB.MenuSelection.SelectionCol.Id()} = {DB.MainTable.MainTableCol.Id()}
                    WHERE 
                            {DB.MainTableLang2.StatusCol.Id(language)} = '{DB.Codes.Yes}'";
            }
        }

        private static string GetMenuLookupFoldersQuery2_3(this SqlDbConfig_23 DB, string language)
        {
            if (!DB.isSecondaryLanguage(language))
            {
                return $@"SELECT 
                            {DB.MenuSelection.MenuCol.ForSelect()}, 
                            {DB.MenuSelection.SelectionCol.ForSelect()}, 
                            {DB.MenuSelection.SelectionCol.ForSelect()} 
                        FROM 
                            {DB.MenuSelection.GetNameAndAlias()}
                        WHERE 
                            {DB.MenuSelection.LevelNoCol.Id()} NOT IN (SELECT {DB.MetaAdm.ValueCol.Id()} FROM {DB.MetaAdm.GetNameAndAlias()} WHERE upper({DB.MetaAdm.PropertyCol.Id()}) = 'MENULEVELS')";
            }
            else
            {
                return $@"SELECT 
                            {DB.MenuSelectionLang2.MenuCol.ForSelect(language)}, 
                            {DB.MenuSelectionLang2.SelectionCol.ForSelect(language)}, 
                            {DB.MenuSelectionLang2.SelectionCol.ForSelect(language)}
                        FROM 
                            {DB.MenuSelectionLang2.GetNameAndAlias(language)} 
                            JOIN {DB.MenuSelection.GetNameAndAlias()} ON {DB.MenuSelectionLang2.MenuCol.Id(language)} = {DB.MenuSelection.MenuCol.Id()} AND {DB.MenuSelectionLang2.SelectionCol.Id(language)} = {DB.MenuSelection.SelectionCol.Id()}
                        WHERE 
                            {DB.MenuSelection.LevelNoCol.Id()} NOT IN (SELECT {DB.MetaAdm.ValueCol.Id()} FROM {DB.MetaAdm.GetNameAndAlias()} WHERE upper({DB.MetaAdm.PropertyCol.Id()}) = 'MENULEVELS')";
            }
        }

        private static string GetMenuLookupTablesQuery2_3(this SqlDbConfig_23 DB, string language)
        {
            if (!DB.isSecondaryLanguage(language))
            {
                return $@"SELECT 
                            {DB.MenuSelection.MenuCol.ForSelect()}, 
                            {DB.MainTable.MainTableCol.ForSelect()}, 
                            {DB.MainTable.TableIdCol.ForSelect()} 
                        FROM 
                            {DB.MainTable.GetNameAndAlias()} 
                            JOIN {DB.MenuSelection.GetNameAndAlias()} ON {DB.MenuSelection.SelectionCol.Id()} = {DB.MainTable.MainTableCol.Id()}";
            }
            else
            {
                return $@"SELECT 
                            {DB.MenuSelection.MenuCol.ForSelect()}, 
                            {DB.MainTable.MainTableCol.ForSelect()}, 
                            {DB.MainTable.TableIdCol.ForSelect()} 
                        FROM 
                            {DB.MainTable.GetNameAndAlias()} 
                            JOIN {DB.SecondaryLanguage.GetNameAndAlias()} ON {DB.MainTable.MainTableCol.Id()} = {DB.SecondaryLanguage.MainTableCol.Id()} 
                            JOIN {DB.MenuSelection.GetNameAndAlias()} ON {DB.MenuSelection.SelectionCol.Id()} = {DB.MainTable.MainTableCol.Id()}
                        WHERE 
                            {DB.SecondaryLanguage.LanguageCol.Id()} = '{language}' AND
                            {DB.SecondaryLanguage.CompletelyTranslatedCol.Id()} = '{DB.Codes.Yes}'";
            }
        }

        private static string GetMenuLookupFoldersQuery2_4(this SqlDbConfig_24 DB, string language)
        {
            if (!DB.isSecondaryLanguage(language))
            {
                return $@"SELECT 
                            {DB.MenuSelection.MenuCol.ForSelect()}, 
                            {DB.MenuSelection.SelectionCol.ForSelect()}, 
                            {DB.MenuSelection.SelectionCol.ForSelect()} 
                        FROM 
                            {DB.MenuSelection.GetNameAndAlias()}
                        WHERE 
                            {DB.MenuSelection.LevelNoCol.Id()} NOT IN (SELECT {DB.MetaAdm.ValueCol.Id()} FROM {DB.MetaAdm.GetNameAndAlias()} WHERE upper({DB.MetaAdm.PropertyCol.Id()}) = 'MENULEVELS')";
            }
            else
            {
                return $@"SELECT 
                            {DB.MenuSelectionLang2.MenuCol.ForSelect(language)}, 
                            {DB.MenuSelectionLang2.SelectionCol.ForSelect(language)}, 
                            {DB.MenuSelectionLang2.SelectionCol.ForSelect(language)}
                        FROM 
                            {DB.MenuSelectionLang2.GetNameAndAlias(language)} 
                            JOIN {DB.MenuSelection.GetNameAndAlias()} ON {DB.MenuSelectionLang2.MenuCol.Id(language)} = {DB.MenuSelection.MenuCol.Id()} AND {DB.MenuSelectionLang2.SelectionCol.Id(language)} = {DB.MenuSelection.SelectionCol.Id()}
                        WHERE 
                            {DB.MenuSelection.LevelNoCol.Id()} NOT IN (SELECT {DB.MetaAdm.ValueCol.Id()} FROM {DB.MetaAdm.GetNameAndAlias()} WHERE upper({DB.MetaAdm.PropertyCol.Id()}) = 'MENULEVELS')";
            }

        }


        private static string GetMenuLookupTablesQuery2_4(this SqlDbConfig_24 DB, string language)
        {
            if (!DB.isSecondaryLanguage(language))
            {
                return $@"SELECT 
                            {DB.MenuSelection.MenuCol.ForSelect()}, 
                            {DB.MainTable.MainTableCol.ForSelect()}, 
                            {DB.MainTable.TableIdCol.ForSelect()} 
                        FROM 
                            {DB.MainTable.GetNameAndAlias()} 
                            JOIN {DB.MenuSelection.GetNameAndAlias()} ON {DB.MenuSelection.SelectionCol.Id()} = {DB.MainTable.MainTableCol.Id()}";
            }
            else
            {
                return $@"SELECT 
                            {DB.MenuSelection.MenuCol.ForSelect()}, 
                            {DB.MainTable.MainTableCol.ForSelect()}, 
                            {DB.MainTable.TableIdCol.ForSelect()} 
                        FROM 
                            {DB.MainTable.GetNameAndAlias()} 
                            JOIN {DB.SecondaryLanguage.GetNameAndAlias()} ON {DB.MainTable.MainTableCol.Id()} = {DB.SecondaryLanguage.MainTableCol.Id()} 
                            JOIN {DB.MenuSelection.GetNameAndAlias()} ON {DB.MenuSelection.SelectionCol.Id()} = {DB.MainTable.MainTableCol.Id()}
                        WHERE 
                            {DB.SecondaryLanguage.LanguageCol.Id()} = '{language}' AND
                            {DB.SecondaryLanguage.CompletelyTranslatedCol.Id()} = '{DB.Codes.Yes}'";
            }

        }

        public static PxSqlCommand GetPxSqlCommand(SqlDbConfig DB)
        {
            InfoForDbConnection info;

            info = DB.GetInfoForDbConnection(DB.GetDefaultConnString());
            return new PxSqlCommandForTempTables(info.DataBaseType, info.DataProvider, info.ConnectionString);
        }

    }
}

