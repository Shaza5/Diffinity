-- Returns all user-defined stored procedure names
CREATE PROC tools.spGetProcsNames
As
SELECT 
    s.name + '.' + p.name AS FullProcedureName
FROM 
    sys.procedures p
INNER JOIN 
    sys.schemas s ON p.schema_id = s.schema_id
ORDER BY 
    FullProcedureName;

GO

-- Returns the body of a procedure by name
CREATE PROC tools.spGetProcBody
(
    @procName VARCHAR(100)
)
AS
SELECT OBJECT_DEFINITION (OBJECT_ID(@procName)) AS [ProcedureBody]

GO

-- Returns all user-defined stored view names
  CREATE PROC tools.spGetViewsNames
As
	SELECT 
    s.name + '.' + v.name AS FullViewName
FROM 
    sys.views v
INNER JOIN 
    sys.schemas s ON v.schema_id = s.schema_id
ORDER BY 
    FullViewName;


GO

-- Returns the body of a view by name
  CREATE PROC tools.spGetViewBody
(
	@viewName VARCHAR(100)
)
AS
SELECT OBJECT_DEFINITION (OBJECT_ID(@viewName)) AS [ViewBody]


GO
  
-- Returns all user-defined stored table names
CREATE PROC tools.spGetTablesNames
As
	SELECT 
    s.name + '.' + t.name AS FullTableName
FROM 
    sys.tables t
INNER JOIN 
    sys.schemas s ON t.schema_id = s.schema_id
ORDER BY 
    FullTableName;

GO

-- Returns the info of a table by name
CREATE PROC tools.spGetTableInfo
(
 @FullName NVARCHAR(128)
)
AS
SELECT 
    col.COLUMN_NAME columnName,
    col.DATA_TYPE columnType,
    col.IS_NULLABLE isNullable,
    col.CHARACTER_MAXIMUM_LENGTH maxLength,
    CASE WHEN pk.COLUMN_NAME IS NOT NULL THEN 'YES' ELSE 'NO' END AS isPrimaryKey,
    CASE WHEN fk.COLUMN_NAME IS NOT NULL THEN 'YES' ELSE 'NO' END AS isForeignKey
FROM INFORMATION_SCHEMA.COLUMNS col
LEFT JOIN (
    SELECT KU.TABLE_NAME, KU.COLUMN_NAME, KU.TABLE_SCHEMA
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS TC
    JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KU
        ON TC.CONSTRAINT_NAME = KU.CONSTRAINT_NAME
    WHERE TC.CONSTRAINT_TYPE = 'PRIMARY KEY'
) pk ON col.TABLE_NAME = pk.TABLE_NAME AND col.COLUMN_NAME = pk.COLUMN_NAME AND col.TABLE_SCHEMA = pk.TABLE_SCHEMA
LEFT JOIN (
    SELECT CU.TABLE_NAME, CU.COLUMN_NAME, CU.TABLE_SCHEMA
    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS RC
    INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK
        ON RC.CONSTRAINT_NAME = FK.CONSTRAINT_NAME
    INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE CU
        ON CU.CONSTRAINT_NAME = FK.CONSTRAINT_NAME
) fk ON col.TABLE_NAME = fk.TABLE_NAME AND col.COLUMN_NAME = fk.COLUMN_NAME AND col.TABLE_SCHEMA = fk.TABLE_SCHEMA
WHERE col.TABLE_NAME = PARSENAME(@FullName, 1) AND col.TABLE_SCHEMA = PARSENAME(@FullName, 2)
ORDER BY col.ORDINAL_POSITION;
