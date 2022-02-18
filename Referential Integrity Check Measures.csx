#r "C:\Users\berna\OneDrive\Documentos\GitHub\ExtendedTOMWrapper\ExtendedTOMWrapper\bin\x64\Debug\ExtendedTOMWrapper.dll" 
using ExtendedTOMWrapper; 

//modify line 1 of code with the path to the downloaded copy of the custom dll 
//download the latest version of the custom dll from https://github.com/bernatagulloesbrina/ExtendedTOMWrapper/blob/main/bin/x64/Debug/ExtendedTOMWrapper.dll

string overallCounterExpression = "";
string overallCounterName = "Total Unmapped Items";

string overallDetailExpression = "";
string overallDetailName = "Data Problems";

//Table tableToStoreMeasures = Selected.Tables.First();
Table tableToStoreMeasures;
if (
    !TableUtils.CreateMeasureTable(
        createdTable: out tableToStoreMeasures,
        model: Model,
        defaultTableName: "Data Quality Measures 2")
    ) return;

foreach (var r in Model.Relationships)
{


    bool isOneToMany =
        r.FromCardinality == RelationshipEndCardinality.One
        & r.ToCardinality == RelationshipEndCardinality.Many;

    bool isManyToOne =
        r.FromCardinality == RelationshipEndCardinality.Many
        & r.ToCardinality == RelationshipEndCardinality.One;

    Column manyColumn = null as Column;
    Column oneColumn = null as Column;
    bool isOneToManyOrManyToOne = true;
    if (isOneToMany)
    {
        manyColumn = r.ToColumn;
        oneColumn = r.FromColumn;

    }
    else if (isManyToOne)
    {
        manyColumn = r.FromColumn;
        oneColumn = r.ToColumn;
    }
    else
    {
        isOneToManyOrManyToOne = false;
    }

    if (isOneToManyOrManyToOne)
    {

        string orphanCountExpression =
            "CALCULATE("
                + "SUMX(VALUES(" + manyColumn.DaxObjectFullName + "),1),"
                + oneColumn.DaxObjectFullName + " = BLANK()"
            + ")";
        string orphanMeasureName =
            manyColumn.Name + " not mapped in " + manyColumn.Table.Name;

        //Measure newCounter = tableToStoreMeasures.AddMeasure(name: orphanMeasureName, expression: orphanCountExpression,displayFolder:"_Data quality Measures");
        Measure newCounter =
            MeasureUtils.CreateMeasure(
                baseTable: tableToStoreMeasures,
                defaultMeasureName: orphanMeasureName,
                measureExpression: orphanCountExpression,
                displayFolder: "_Data quality Measures",
                allowCustomName: false,
                createMode: MeasureUtils.CreateMode.UseExisting);

        string orphanTableTitleMeasureExpression = newCounter.DaxObjectFullName + " & \" " + newCounter.Name + "\"";
        string orphanTableTitleMeasureName = newCounter.Name + " Title";

        //Measure newTitle = tableToStoreMeasures.AddMeasure(name: orphanTableTitleMeasureName, expression: orphanTableTitleMeasureExpression, displayFolder: "_Data quality Titles");

        Measure newTitle =
            MeasureUtils.CreateMeasure(
                baseTable: tableToStoreMeasures,
                defaultMeasureName: orphanTableTitleMeasureName,
                measureExpression: orphanTableTitleMeasureExpression,
                displayFolder: "_Data quality Titles",
                allowCustomName: false,
                createMode:MeasureUtils.CreateMode.UseExisting);

        overallCounterExpression = overallCounterExpression + "+" + newCounter.DaxObjectFullName;
        overallDetailExpression = overallDetailExpression
                + " & IF(" + newCounter.DaxObjectFullName + "> 0,"
                            + newTitle.DaxObjectFullName + " UNICHAR(10))";

    };

};

//tableToStoreMeasures.AddMeasure(name: overallCounterName, expression: overallCounterExpression);
MeasureUtils.CreateMeasure(
    baseTable: tableToStoreMeasures, 
    defaultMeasureName: overallCounterName, 
    measureExpression: overallCounterExpression, 
    allowCustomName: false,
    createMode: MeasureUtils.CreateMode.DeleteAndCreate);

//tableToStoreMeasures.AddMeasure(name: overallDetailName, expression: overallDetailExpression);
MeasureUtils.CreateMeasure(
    baseTable: tableToStoreMeasures, 
    defaultMeasureName: overallDetailName,
    allowCustomName: false,
    createMode: MeasureUtils.CreateMode.DeleteAndCreate);