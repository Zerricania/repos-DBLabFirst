CREATE TABLE Discounts
(
    Id int IDENTITY(1,1) PRIMARY KEY,
    DName nvarchar(50) NOT NULL,
    Percents int NOT NULL
);

CREATE TABLE Customers
(
    Id int IDENTITY(1,1) PRIMARY KEY,
    FullName nvarchar(100) NOT NULL,
    Phone nvarchar(20),
    DiscountId int NULL,

    CONSTRAINT FK_Customers_Discounts
        FOREIGN KEY (DiscountId)
        REFERENCES Discounts(Id)
);

CREATE TABLE SoldBooks
(
    Id int IDENTITY(1,1) PRIMARY KEY,
    BookId int NOT NULL,
    CustomerId int NOT NULL,
    SaleDate date NOT NULL,
    Quantity int NOT NULL,
    Price money NOT NULL,

    CONSTRAINT FK_SoldBooks_Books
        FOREIGN KEY (BookId)
        REFERENCES Books(Id),

    CONSTRAINT FK_SoldBooks_Customers
        FOREIGN KEY (CustomerId)
        REFERENCES Customers(Id)
);