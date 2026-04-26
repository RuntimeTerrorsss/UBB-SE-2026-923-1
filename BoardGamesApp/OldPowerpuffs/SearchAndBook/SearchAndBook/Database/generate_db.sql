USE master;
GO

IF DB_ID(N'BoardGamesRentMockDb') IS NOT NULL
BEGIN
    ALTER DATABASE BoardGamesRentMockDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE BoardGamesRentMockDb;
END
GO

CREATE DATABASE BoardGamesRentMockDb;
GO

USE BoardGamesRentMockDb;
GO

IF OBJECT_ID(N'dbo.Rentals', N'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Rentals;
END
GO

IF OBJECT_ID(N'dbo.Games', N'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Games;
END
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Users;
END
GO

CREATE TABLE dbo.Users
(
    user_id INT NOT NULL PRIMARY KEY,
    username VARCHAR(50) NOT NULL,
    display_name VARCHAR(50) NOT NULL,
    email VARCHAR(100) NOT NULL,
    password_hash VARCHAR(100) NOT NULL,
    phone_number VARCHAR(20) NULL,
    avatar_url VARCHAR(255) NULL,
    is_suspended BIT NOT NULL,
    created_at DATETIME NOT NULL,
    updated_at DATETIME NULL,
    street_name VARCHAR(50) NULL,
    street_number VARCHAR(10) NULL,
    city VARCHAR(100) NOT NULL,
    country VARCHAR(100) NOT NULL
);
GO

CREATE TABLE dbo.Games
(
    game_id INT NOT NULL PRIMARY KEY,
    name VARCHAR(30) NOT NULL,
    price DECIMAL(10, 2) NOT NULL,
    minimum_player_number INT NOT NULL,
    maximum_player_number INT NOT NULL,
    description VARCHAR(500) NOT NULL,
    image VARBINARY(MAX) NULL,
    is_active BIT NOT NULL,
    owner_id INT NOT NULL,

    CONSTRAINT CK_Games_Price_Positive
        CHECK (price > 0),

    CONSTRAINT CK_Games_Player_Range
        CHECK (
            minimum_player_number > 0
            AND maximum_player_number > 0
            AND minimum_player_number <= maximum_player_number
        ),

    CONSTRAINT FK_Games_Users_Owner
        FOREIGN KEY (owner_id) REFERENCES dbo.Users(user_id)
);
GO

CREATE TABLE dbo.Rentals
(
    rental_id INT NOT NULL PRIMARY KEY,
    game_id INT NOT NULL,
    renter_id INT NOT NULL,
    owner_id INT NOT NULL,
    start_date DATETIME NOT NULL,
    end_date DATETIME NOT NULL,
    total_price DECIMAL(10, 2) NULL,

    CONSTRAINT CK_Rentals_Date_Range
        CHECK (start_date < end_date),

    CONSTRAINT CK_Rentals_Total_Price_NonNegative
        CHECK (total_price IS NULL OR total_price >= 0),

    CONSTRAINT FK_Rentals_Games
        FOREIGN KEY (game_id) REFERENCES dbo.Games(game_id),

    CONSTRAINT FK_Rentals_Users_Renter
        FOREIGN KEY (renter_id) REFERENCES dbo.Users(user_id),

    CONSTRAINT FK_Rentals_Users_Owner
        FOREIGN KEY (owner_id) REFERENCES dbo.Users(user_id)
);
GO

INSERT INTO dbo.Users
(
    user_id,
    username,
    display_name,
    email,
    password_hash,
    phone_number,
    avatar_url,
    is_suspended,
    created_at,
    updated_at,
    street_name,
    street_number,
    city,
    country
)
VALUES
(1, 'alice01', 'Alice', 'alice@example.com', 'hash1', '0711111111', 'https://media.istockphoto.com/id/1437816897/photo/business-woman-manager-or-human-resources-portrait-for-career-success-company-we-are-hiring.jpg?s=612x612&w=0&k=20&c=tyLvtzutRh22j9GqSGI33Z4HpIwv9vL_MZw_xOE19NQ=', 0, GETDATE(), GETDATE(), 'Main Street', '10', 'Cluj-Napoca', 'Romania'),
(2, 'bob02', 'Bob', 'bob@example.com', 'hash2', '0722222222', 'https://img.freepik.com/free-photo/portrait-white-man-isolated_53876-40306.jpg?semt=ais_hybrid&w=740&q=80', 0, GETDATE(), GETDATE(), 'Liberty Street', '21', 'Constanta', 'Romania'),
(3, 'carol03', 'Carol', 'carol@example.com', 'hash3', '0733333333', 'https://st4.depositphotos.com/4157265/40368/i/450/depositphotos_403682066-stock-photo-profile-picture-of-smiling-caucasian.jpg', 0, GETDATE(), GETDATE(), 'Oak Street', '5', 'Oradea', 'Romania'),
(4, 'david04', 'David', 'david@example.com', 'hash4', '0744444444', 'https://t3.ftcdn.net/jpg/06/99/46/60/360_F_699466075_DaPTBNlNQTOwwjkOiFEoOvzDV0ByXR9E.jpg', 0, GETDATE(), GETDATE(), 'River Street', '12', 'Suceava', 'Romania'),
(5, 'emma05', 'Emma', 'emma@example.com', 'hash5', '0755555555', 'https://media.istockphoto.com/id/1471845315/photo/happy-portrait-or-business-woman-taking-a-selfie-in-office-building-for-a-social-media.jpg?s=612x612&w=0&k=20&c=AOylBL01joI0zphCAFr6YVrsOgp_jd2XtVUychLXYho=', 0, GETDATE(), GETDATE(), 'Forest Street', '7', 'Bucharest', 'Romania'),
(6, 'frank06', 'Frank', 'frank@example.com', 'hash6', '0766666666', 'https://st2.depositphotos.com/4157265/43642/i/450/depositphotos_436429026-stock-photo-happy-young-man-posing-for.jpg', 0, GETDATE(), GETDATE(), 'Sunset Blvd', '45', 'Timisoara', 'Romania'),
(7, 'grace07', 'Grace', 'grace@example.com', 'hash7', '0777777777', 'https://i.pinimg.com/736x/97/30/df/9730dfdfd89550c365aaacc9760815b1.jpg', 0, GETDATE(), GETDATE(), 'Hill Road', '3', 'Iasi', 'Romania'),
(8, 'henry08', 'Henry', 'henry@example.com', 'hash8', '0788888888', 'https://files.idyllic.app/files/static/3929121?width=256&optimizer=image', 0, GETDATE(), GETDATE(), 'Lake Street', '19', 'Brasov', 'Romania');
GO

INSERT INTO dbo.Games
(
    game_id,
    name,
    price,
    minimum_player_number,
    maximum_player_number,
    description,
    image,
    is_active,
    owner_id
)
VALUES
(1, 'Catan', 15.00, 3, 4, 'Trade and build on the island of Catan.', NULL, 1, 1),
(2, 'Monopoly', 10.00, 2, 6, 'Classic property trading game.', NULL, 1, 2),
(3, 'Carcassonne', 12.50, 2, 5, 'Tile placement game.', NULL, 1, 1),
(4, 'Terraforming Mars', 20.00, 1, 5, 'Strategy game about developing Mars.', NULL, 0, 3);
GO

INSERT INTO dbo.Rentals
(
    rental_id,
    game_id,
    renter_id,
    owner_id,
    start_date,
    end_date,
    total_price
)
VALUES
(1, 1, 2, 1, '2026-05-10T00:00:00', '2026-05-15T00:00:00', 75.00),
(2, 2, 1, 2, '2026-05-20T00:00:00', '2026-05-22T00:00:00', 20.00),
(3, 1, 1, 2, '2026-04-01T00:00:00', '2026-04-05T00:00:00', 20.00);
GO

-- add 30 more games
INSERT INTO dbo.Games (
    game_id, name, price, minimum_player_number, maximum_player_number,
    description, image, is_active, owner_id
)
VALUES
(5, 'Ticket to Ride', 13.50, 2, 5, 'Build railway routes across the world.', NULL, 1, 1),
(6, 'Pandemic', 14.00, 2, 4, 'Work together to stop global outbreaks.', NULL, 1, 2),
(7, '7 Wonders', 16.00, 2, 7, 'Build a civilization and wonders.', NULL, 1, 3),
(8, 'Azul', 11.00, 2, 4, 'Decorate the royal palace walls.', NULL, 1, 1),
(9, 'Dixit', 10.50, 3, 6, 'Creative storytelling game.', NULL, 1, 2),
--
(10, 'Splendor', 12.00, 2, 4, 'Build your gem empire.', NULL, 1, 8),
(11, 'Codenames', 9.00, 2, 8, 'Team word guessing game.', NULL, 1, 6),
(12, 'Risk', 11.50, 2, 6, 'Classic world domination game.', NULL, 1, 5),
(13, 'Dominion', 13.00, 2, 4, 'Deck-building strategy game.', NULL, 1, 7),
(14, 'Love Letter', 7.50, 2, 4, 'Quick deduction card game.', NULL, 1, 8),
(15, 'Scythe', 22.00, 1, 5, 'Strategy game in alternate history.', NULL, 1, 2),
(16, 'Wingspan', 18.00, 1, 5, 'Build a bird sanctuary.', NULL, 1, 3),
(17, 'Gloomhaven', 25.00, 1, 4, 'Epic campaign dungeon crawler.', NULL, 1, 1),
(18, 'Brass Birmingham', 21.00, 2, 4, 'Industrial revolution strategy.', NULL, 1, 8),
(19, 'Root', 17.50, 2, 4, 'Asymmetric woodland warfare.', NULL, 1, 8),
(20, 'Terraforming Mars: Ares', 19.00, 1, 4, 'Faster Mars engine builder.', NULL, 1, 7),
(21, 'Ark Nova', 23.00, 1, 4, 'Build the best zoo.', NULL, 1, 6),
(22, 'Everdell', 16.50, 1, 4, 'Build a forest civilization.', NULL, 1, 6),
(23, 'The Crew', 9.50, 2, 5, 'Cooperative trick-taking game.', NULL, 1, 4),
(24, 'Hanabi', 8.00, 2, 5, 'Play cards without seeing them.', NULL, 1, 4),
(25, 'Agricola', 17.00, 1, 4, 'Farm-building strategy game.', NULL, 1, 4),
(26, 'Patchwork', 10.00, 2, 2, 'Two-player quilt game.', NULL, 1, 5),
(27, 'Carcassonne: Expansion', 13.50, 2, 6, 'Expand the classic Carcassonne.', NULL, 1, 6),
(28, 'Uno', 5.00, 2, 6, 'Classic card shedding game.', NULL, 1, 3),
(29, 'Exploding Kittens', 8.50, 2, 5, 'Explosive card game.', NULL, 1, 1),
(30, 'Bang!', 9.00, 4, 7, 'Wild west bluffing game.', NULL, 1, 2),
(31, 'King of Tokyo', 12.50, 2, 6, 'Monster battle dice game.', NULL, 1, 3),
(32, 'Sheriff of Nottingham', 14.50, 3, 5, 'Bluff and smuggle goods.', NULL, 1, 1),
(33, 'Mysterium', 15.50, 2, 7, 'Solve a ghost mystery.', NULL, 1, 5),
(34, 'Clank!', 16.00, 2, 4, 'Deck-building dungeon crawl.', NULL, 1, 7);

SELECT COUNT(*) AS UserCount FROM dbo.Users;
SELECT COUNT(*) AS GameCount FROM dbo.Games;
SELECT COUNT(*) AS RentalCount FROM dbo.Rentals;