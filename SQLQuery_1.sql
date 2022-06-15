use Vendtechnov
go

-- alter table UserAssignedModules
-- add IsAddedFromAgency bit DEFAULT (0)

-- alter table UserAssignedWidgets
-- add IsAddedFromAgency bit DEFAULT (0)

-- update UserAssignedWidgets set IsAddedFromAgency = 0
-- update UserAssignedModules set IsAddedFromAgency = 0

-- update UserAssignedWidgets set IsAddedFromAgency = 1 where WidgetId = 10

update [UserAssignedWidgets]
set IsAddedFromAgency = 1
where [UserAssignedWidgets].[ModuleId] = 26

select * from Modules

-- SELECT  IsAddedFromAgency from UserAssignedModules
-- SELECT  IsAddedFromAgency from UserAssignedWidgets