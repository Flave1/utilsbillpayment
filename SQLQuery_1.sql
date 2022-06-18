use abc_production
go

--deploy wen.config file to live


-- alter table UserAssignedModules
-- add IsAddedFromAgency bit DEFAULT (0)

-- alter table UserAssignedWidgets
-- add IsAddedFromAgency bit DEFAULT (0)

-- update UserAssignedWidgets set IsAddedFromAgency = 0
-- update UserAssignedModules set IsAddedFromAgency = 0

-- update UserAssignedWidgets set IsAddedFromAgency = 1 where WidgetId = 10

-- update [UserAssignedModules]
-- set IsAddedFromAgency = 1
-- where moduleId = 33
-- select * from Modules

-- alter table deposits
-- add ValueDateStamp DateTime null

--  alter table PendingDeposits
--  add ValueDateStamp DateTime null
