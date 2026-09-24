CREATE TABLE Books (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(50),
    Author NVARCHAR(20),
    Izdatelstvo NVARCHAR(30),
    Year INT,
    Price MONEY,
    Janr NVARCHAR(20),
    Remarks NVARCHAR(50)
);
