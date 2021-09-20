# SSISAnalyzer
Checks SSIS packages on embedded SQL
Having Embedded SQL inside SSIS is a real pain in the ass when you need to improve performance of existing packages. You need to open and examine the SQL code.
This tool allows you to go through the various dtsx files and save the SQL behind the Execute SQL tasks, Data Flow Sources etc. 
It's quite rudimentary but the program spits out text files that can be used to create stored procedures in SQL Server.
My main goal was to have the SQL code in SQL Server so I no longer have to worry if there is shady SQL code running somewhere. 
If you have questions on the code, don't hesitate to contact me through github.
