use abc_production
go
select * from Users

--UPDATE live SET companyname = dev.companyname, vendor = dev.vendor, Address = dev.Address

--FROM Vendtechnov.dbo.Users AS dev

--     INNER JOIN ABC_PRODUCTION.dbo.Users AS live

--        ON dev.UserId = live.UserId

SELECT UserId, name, SurName, vendor FROM ABC_PRODUCTION.dbo.Users where UserId not in  (SELECT UserId from VENDTECHNOV.dbo.Users)

select * from Pos

