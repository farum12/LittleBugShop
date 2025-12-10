using System.Collections.Generic;
using LittleBugShop.Models;

namespace LittleBugShop.Data
{
    public static class Database
    {
        public static List<Order> Orders { get; } = new List<Order>();
        public static List<Cart> Carts { get; } = new List<Cart>();
        public static List<Review> Reviews { get; } = new List<Review>();
        public static List<ReviewHelpful> ReviewHelpfuls { get; } = new List<ReviewHelpful>();
        public static List<Address> Addresses { get; } = new List<Address>();
        public static List<Wishlist> Wishlists { get; } = new List<Wishlist>();
        public static List<Coupon> Coupons { get; } = new List<Coupon>();
        public static List<CouponUsage> CouponUsages { get; } = new List<CouponUsage>();
        public static List<PaymentMethod> PaymentMethods { get; } = new List<PaymentMethod>();
        public static List<PaymentTransaction> PaymentTransactions { get; } = new List<PaymentTransaction>();
        public static List<Product> Products { get; } = new List<Product>()
            {
            new Product { Id = 1, Name = "The Great Gatsby", Author = "F. Scott Fitzgerald", Genre = "Classic Fiction", ISBN = "978-0743273565", Price = 10.99m, Description = "A novel written by American author F. Scott Fitzgerald.", Type = "Book", StockQuantity = 15, LowStockThreshold = 5 },
            new Product { Id = 2, Name = "1984", Author = "George Orwell", Genre = "Dystopian Fiction", ISBN = "978-0451524935", Price = 8.99m, Description = "A dystopian social science fiction novel and cautionary tale, written by the English writer George Orwell.", Type = "Book", StockQuantity = 20, LowStockThreshold = 5 },
            new Product { Id = 3, Name = "To Kill a Mockingbird", Author = "Harper Lee", Genre = "Classic Fiction", ISBN = "978-0061120084", Price = 7.99m, Description = "A novel by Harper Lee published in 1960. Instantly successful, widely read in high schools and middle schools in the United States.", Type = "Book", StockQuantity = 12, LowStockThreshold = 5 },
            new Product { Id = 4, Name = "The Catcher in the Rye", Author = "J. D. Salinger", Genre = "Classic Fiction", ISBN = "978-0316769488", Price = 6.99m, Description = "A novel by J. D. Salinger, partially published in serial form in 1945–1946 and as a novel in 1951.", Type = "Book", StockQuantity = 8, LowStockThreshold = 5 },
            new Product { Id = 5, Name = "Pride and Prejudice", Author = "Jane Austen", Genre = "Romance", ISBN = "978-0141439518", Price = 9.99m, Description = "A romantic novel of manners written by Jane Austen in 1813.", Type = "Book", StockQuantity = 25, LowStockThreshold = 5 },
            new Product { Id = 6, Name = "Moby-Dick", Author = "Herman Melville", Genre = "Adventure", ISBN = "978-1503280786", Price = 11.99m, Description = "A novel by Herman Melville, published in 1851 during the period of the American Renaissance.", Type = "Book", StockQuantity = 3, LowStockThreshold = 5 },
            new Product { Id = 7, Name = "War and Peace", Author = "Leo Tolstoy", Genre = "Historical Fiction", ISBN = "978-0199232765", Price = 12.99m, Description = "A novel by the Russian author Leo Tolstoy, published from 1865 to 1869.", Type = "Book", StockQuantity = 10, LowStockThreshold = 5 },
            new Product { Id = 8, Name = "The Odyssey", Author = "Homer", Genre = "Epic Poetry", ISBN = "978-0140268867", Price = 13.99m, Description = "An ancient Greek epic poem attributed to Homer.", Type = "Book", StockQuantity = 7, LowStockThreshold = 5 },
            new Product { Id = 9, Name = "Crime and Punishment", Author = "Fyodor Dostoevsky", Genre = "Psychological Fiction", ISBN = "978-0486415871", Price = 14.99m, Description = "A novel by the Russian author Fyodor Dostoevsky.", Type = "Book", StockQuantity = 14, LowStockThreshold = 5 },
            new Product { Id = 10, Name = "The Brothers Karamazov", Author = "Fyodor Dostoevsky", Genre = "Psychological Fiction", ISBN = "978-0374528379", Price = 15.99m, Description = "A novel by the Russian author Fyodor Dostoevsky.", Type = "Book", StockQuantity = 6, LowStockThreshold = 5 },
            new Product { Id = 11, Name = "Brave New World", Author = "Aldous Huxley", Genre = "Dystopian Fiction", ISBN = "978-0060850524", Price = 16.99m, Description = "A dystopian social science fiction novel by English author Aldous Huxley.", Type = "Book", StockQuantity = 18, LowStockThreshold = 5 },
            new Product { Id = 12, Name = "Jane Eyre", Author = "Charlotte Brontë", Genre = "Romance", ISBN = "978-0141441146", Price = 17.99m, Description = "A novel by English writer Charlotte Brontë, published under the pen name 'Currer Bell'.", Type = "Book", StockQuantity = 22, LowStockThreshold = 5 },
            new Product { Id = 13, Name = "Wuthering Heights", Author = "Emily Brontë", Genre = "Gothic Fiction", ISBN = "978-0141439556", Price = 18.99m, Description = "A novel by Emily Brontë published in 1847 under her pseudonym Ellis Bell.", Type = "Book", StockQuantity = 11, LowStockThreshold = 5 },
            new Product { Id = 14, Name = "The Divine Comedy", Author = "Dante Alighieri", Genre = "Epic Poetry", ISBN = "978-0142437223", Price = 19.99m, Description = "An Italian narrative poem by Dante Alighieri, begun in 1308 and completed in 1320.", Type = "Book", StockQuantity = 5, LowStockThreshold = 5 },
            new Product { Id = 15, Name = "The Hobbit", Author = "J. R. R. Tolkien", Genre = "Fantasy", ISBN = "978-0547928227", Price = 20.99m, Description = "A children's fantasy novel by English author J. R. R. Tolkien.", Type = "Book", StockQuantity = 30, LowStockThreshold = 5 },
            new Product { Id = 16, Name = "The Lord of the Rings", Author = "J. R. R. Tolkien", Genre = "Fantasy", ISBN = "978-0544003415", Price = 21.99m, Description = "An epic high-fantasy novel by English author and scholar J. R. R. Tolkien.", Type = "Book", StockQuantity = 28, LowStockThreshold = 5 },
            new Product { Id = 17, Name = "Harry Potter and the Sorcerer's Stone", Author = "J. K. Rowling", Genre = "Fantasy", ISBN = "978-0590353427", Price = 22.99m, Description = "A fantasy novel written by British author J. K. Rowling.", Type = "Book", StockQuantity = 50, LowStockThreshold = 10 },
            new Product { Id = 18, Name = "Harry Potter and the Chamber of Secrets", Author = "J. K. Rowling", Genre = "Fantasy", ISBN = "978-0439064873", Price = 23.99m, Description = "A fantasy novel written by British author J. K. Rowling.", Type = "Book", StockQuantity = 45, LowStockThreshold = 10 },
            new Product { Id = 19, Name = "Harry Potter and the Prisoner of Azkaban", Author = "J. K. Rowling", Genre = "Fantasy", ISBN = "978-0439136365", Price = 24.99m, Description = "A fantasy novel written by British author J. K. Rowling.", Type = "Book", StockQuantity = 42, LowStockThreshold = 10 },
            new Product { Id = 20, Name = "Harry Potter and the Goblet of Fire", Author = "J. K. Rowling", Genre = "Fantasy", ISBN = "978-0439139601", Price = 25.99m, Description = "A fantasy novel written by British author J. K. Rowling.", Type = "Book", StockQuantity = 38, LowStockThreshold = 10 },
            new Product { Id = 21, Name = "Harry Potter and the Order of the Phoenix", Author = "J. K. Rowling", Genre = "Fantasy", ISBN = "978-0439358071", Price = 26.99m, Description = "A fantasy novel written by British author J. K. Rowling.", Type = "Book", StockQuantity = 35, LowStockThreshold = 10 },
            new Product { Id = 22, Name = "Harry Potter and the Half-Blood Prince", Author = "J. K. Rowling", Genre = "Fantasy", ISBN = "978-0439785969", Price = 27.99m, Description = "A fantasy novel written by British author J. K. Rowling.", Type = "Book", StockQuantity = 32, LowStockThreshold = 10 },
            new Product { Id = 23, Name = "Harry Potter and the Deathly Hallows", Author = "J. K. Rowling", Genre = "Fantasy", ISBN = "978-0545139700", Price = 28.99m, Description = "A fantasy novel written by British author J. K. Rowling.", Type = "Book", StockQuantity = 40, LowStockThreshold = 10 },
            new Product { Id = 24, Name = "The Chronicles of Narnia", Author = "C. S. Lewis", Genre = "Fantasy", ISBN = "978-0066238500", Price = 29.99m, Description = "A series of seven fantasy novels by British author C. S. Lewis.", Type = "Book", StockQuantity = 24, LowStockThreshold = 5 },
            new Product { Id = 25, Name = "The Hunger Games", Author = "Suzanne Collins", Genre = "Dystopian Fiction", ISBN = "978-0439023528", Price = 30.99m, Description = "A dystopian novel by the American writer Suzanne Collins.", Type = "Book", StockQuantity = 33, LowStockThreshold = 10 },
            new Product { Id = 26, Name = "Catching Fire", Author = "Suzanne Collins", Genre = "Dystopian Fiction", ISBN = "978-0439023498", Price = 31.99m, Description = "A dystopian novel by the American writer Suzanne Collins.", Type = "Book", StockQuantity = 30, LowStockThreshold = 10 },
            new Product { Id = 27, Name = "Mockingjay", Author = "Suzanne Collins", Genre = "Dystopian Fiction", ISBN = "978-0439023511", Price = 32.99m, Description = "A dystopian novel by the American writer Suzanne Collins.", Type = "Book", StockQuantity = 28, LowStockThreshold = 10 },
            new Product { Id = 28, Name = "The Maze Runner", Author = "James Dashner", Genre = "Young Adult", ISBN = "978-0385737951", Price = 33.99m, Description = "A young adult dystopian science fiction novel written by American author James Dashner.", Type = "Book", StockQuantity = 19, LowStockThreshold = 5 },
            new Product { Id = 29, Name = "The Scorch Trials", Author = "James Dashner", Genre = "Young Adult", ISBN = "978-0385738767", Price = 34.99m, Description = "A young adult dystopian science fiction novel written by American author James Dashner.", Type = "Book", StockQuantity = 16, LowStockThreshold = 5 },
            new Product { Id = 30, Name = "The Death Cure", Author = "James Dashner", Genre = "Young Adult", ISBN = "978-0385738774", Price = 35.99m, Description = "A young adult dystopian science fiction novel written by American author James Dashner.", Type = "Book", StockQuantity = 14, LowStockThreshold = 5 },
            new Product { Id = 31, Name = "Divergent", Author = "Veronica Roth", Genre = "Young Adult", ISBN = "978-0062024039", Price = 36.99m, Description = "A dystopian novel by the American author Veronica Roth.", Type = "Book", StockQuantity = 21, LowStockThreshold = 5 },
            new Product { Id = 32, Name = "Insurgent", Author = "Veronica Roth", Genre = "Young Adult", ISBN = "978-0062024053", Price = 37.99m, Description = "A dystopian novel by the American author Veronica Roth.", Type = "Book", StockQuantity = 18, LowStockThreshold = 5 },
            new Product { Id = 33, Name = "Allegiant", Author = "Veronica Roth", Genre = "Young Adult", ISBN = "978-0062024077", Price = 38.99m, Description = "A dystopian novel by the American author Veronica Roth.", Type = "Book", StockQuantity = 15, LowStockThreshold = 5 },
            new Product { Id = 34, Name = "The Fault in Our Stars", Author = "John Green", Genre = "Young Adult", ISBN = "978-0142424179", Price = 39.99m, Description = "A novel by John Green.", Type = "Book", StockQuantity = 0, LowStockThreshold = 5 },
            new Product { Id = 35, Name = "Looking for Alaska", Author = "John Green", Genre = "Young Adult", ISBN = "978-0142402511", Price = 40.99m, Description = "A novel by John Green.", Type = "Book", StockQuantity = 2, LowStockThreshold = 5 }
        };
        public static List<User> Users { get; } = new List<User>
            {
                new User { Id = 1, Username = "admin", Password = "admin123", Role = "Admin", Email = "admin@littlebugshop.com", FirstName = "Admin", LastName = "User", PhoneNumber = "+1-555-0100", CreatedAt = DateTime.UtcNow.AddMonths(-6), UpdatedAt = DateTime.UtcNow.AddMonths(-6), AddressIds = new List<int> { 1 } },
                new User { Id = 2, Username = "User", Password = "qazwsxedcrfv12345", Role = "User", Email = "user@example.com", FirstName = "John", LastName = "Doe", PhoneNumber = "+1-555-0101", CreatedAt = DateTime.UtcNow.AddMonths(-3), UpdatedAt = DateTime.UtcNow.AddMonths(-3), AddressIds = new List<int> { 2, 3 } },
                new User { Id = 3, Username = "User2", Password = "password2", Role = "User", Email = "user2@example.com", FirstName = "Jane", LastName = "Smith", PhoneNumber = "+1-555-0102", CreatedAt = DateTime.UtcNow.AddMonths(-2), UpdatedAt = DateTime.UtcNow.AddMonths(-2), AddressIds = new List<int> { 4 } }
            };

        static Database()
        {
            // Seed initial addresses
            Addresses.AddRange(new List<Address>
            {
                new Address { Id = 1, UserId = 1, AddressType = AddressType.Both, Street = "123 Admin Street", City = "New York", State = "NY", PostalCode = "10001", Country = "USA", IsDefault = true },
                new Address { Id = 2, UserId = 2, AddressType = AddressType.Shipping, Street = "456 Oak Avenue", City = "Los Angeles", State = "CA", PostalCode = "90001", Country = "USA", IsDefault = true },
                new Address { Id = 3, UserId = 2, AddressType = AddressType.Billing, Street = "789 Pine Road", City = "Los Angeles", State = "CA", PostalCode = "90002", Country = "USA", IsDefault = false },
                new Address { Id = 4, UserId = 3, AddressType = AddressType.Both, Street = "321 Maple Lane", City = "Chicago", State = "IL", PostalCode = "60601", Country = "USA", IsDefault = true }
            });

            // Seed initial coupons
            var now = DateTime.UtcNow;
            Coupons.AddRange(new List<Coupon>
            {
                new Coupon { Id = 1, Code = "SAVE10", Type = DiscountType.Percentage, Value = 10, ExpirationDate = null, MaxUsesTotal = null, IsActive = true, CurrentUses = 0, CreatedAt = now.AddDays(-30) },
                new Coupon { Id = 2, Code = "WELCOME5", Type = DiscountType.FixedAmount, Value = 5.00m, ExpirationDate = null, MaxUsesTotal = null, IsActive = true, CurrentUses = 0, CreatedAt = now.AddDays(-30) },
                new Coupon { Id = 3, Code = "WINTER20", Type = DiscountType.Percentage, Value = 20, ExpirationDate = now.AddDays(30), MaxUsesTotal = 100, IsActive = true, CurrentUses = 15, CreatedAt = now.AddDays(-15) },
                new Coupon { Id = 4, Code = "EXPIRED", Type = DiscountType.Percentage, Value = 15, ExpirationDate = now.AddDays(-5), MaxUsesTotal = null, IsActive = true, CurrentUses = 25, CreatedAt = now.AddDays(-60) },
                new Coupon { Id = 5, Code = "LIMITED50", Type = DiscountType.FixedAmount, Value = 10.00m, ExpirationDate = null, MaxUsesTotal = 50, IsActive = true, CurrentUses = 50, CreatedAt = now.AddDays(-20) },
                new Coupon { Id = 6, Code = "INACTIVE", Type = DiscountType.Percentage, Value = 25, ExpirationDate = null, MaxUsesTotal = null, IsActive = false, CurrentUses = 0, CreatedAt = now.AddDays(-10) }
            });

            // Seed some initial reviews
            var baseDate = DateTime.UtcNow.AddDays(-30);

            Reviews.AddRange(new List<Review>
            {
                // Harry Potter Sorcerer's Stone (Product 17) - Multiple reviews
                new Review { Id = 1, ProductId = 17, UserId = 2, UserName = "User", Rating = 5, ReviewText = "A magical journey! This book captivated me from the very first page. Perfect introduction to the wizarding world.", IsVerifiedPurchase = false, HelpfulCount = 12, IsHidden = false, CreatedAt = baseDate.AddDays(-25), UpdatedAt = baseDate.AddDays(-25) },
                new Review { Id = 2, ProductId = 17, UserId = 3, UserName = "User2", Rating = 5, ReviewText = "My child loved this book and so did I! Great for all ages.", IsVerifiedPurchase = false, HelpfulCount = 8, IsHidden = false, CreatedAt = baseDate.AddDays(-20), UpdatedAt = baseDate.AddDays(-20) },
                new Review { Id = 3, ProductId = 17, UserId = 1, UserName = "admin", Rating = 4, ReviewText = "Excellent start to the series. A bit slow in places but overall fantastic.", IsVerifiedPurchase = false, HelpfulCount = 5, IsHidden = false, CreatedAt = baseDate.AddDays(-15), UpdatedAt = baseDate.AddDays(-15) },

                // Harry Potter Chamber of Secrets (Product 18) - Good reviews
                new Review { Id = 4, ProductId = 18, UserId = 2, UserName = "User", Rating = 5, ReviewText = "Even better than the first! The mystery kept me guessing until the end.", IsVerifiedPurchase = false, HelpfulCount = 10, IsHidden = false, CreatedAt = baseDate.AddDays(-24), UpdatedAt = baseDate.AddDays(-24) },
                new Review { Id = 5, ProductId = 18, UserId = 3, UserName = "User2", Rating = 4, ReviewText = "Great continuation of the series. Can't wait to read the next one!", IsVerifiedPurchase = false, HelpfulCount = 6, IsHidden = false, CreatedAt = baseDate.AddDays(-18), UpdatedAt = baseDate.AddDays(-18) },

                // The Hobbit (Product 15) - Mixed reviews
                new Review { Id = 6, ProductId = 15, UserId = 1, UserName = "admin", Rating = 5, ReviewText = "A timeless classic! Tolkien's world-building is unparalleled.", IsVerifiedPurchase = false, HelpfulCount = 15, IsHidden = false, CreatedAt = baseDate.AddDays(-28), UpdatedAt = baseDate.AddDays(-28) },
                new Review { Id = 7, ProductId = 15, UserId = 2, UserName = "User", Rating = 4, ReviewText = "Wonderful adventure story, though the pacing is a bit slow at times.", IsVerifiedPurchase = false, HelpfulCount = 7, IsHidden = false, CreatedAt = baseDate.AddDays(-22), UpdatedAt = baseDate.AddDays(-22) },
                new Review { Id = 8, ProductId = 15, UserId = 3, UserName = "User2", Rating = 5, ReviewText = "Perfect blend of adventure and fantasy. Highly recommend!", IsVerifiedPurchase = false, HelpfulCount = 9, IsHidden = false, CreatedAt = baseDate.AddDays(-16), UpdatedAt = baseDate.AddDays(-16) },

                // The Great Gatsby (Product 1) - Classic literature reviews
                new Review { Id = 9, ProductId = 1, UserId = 2, UserName = "User", Rating = 4, ReviewText = "A brilliant portrayal of the American Dream. Fitzgerald's prose is beautiful.", IsVerifiedPurchase = false, HelpfulCount = 11, IsHidden = false, CreatedAt = baseDate.AddDays(-27), UpdatedAt = baseDate.AddDays(-27) },
                new Review { Id = 10, ProductId = 1, UserId = 3, UserName = "User2", Rating = 3, ReviewText = "Well-written but not my favorite. Characters are hard to relate to.", IsVerifiedPurchase = false, HelpfulCount = 4, IsHidden = false, CreatedAt = baseDate.AddDays(-19), UpdatedAt = baseDate.AddDays(-19) },

                // To Kill a Mockingbird (Product 2) - High ratings
                new Review { Id = 11, ProductId = 2, UserId = 1, UserName = "admin", Rating = 5, ReviewText = "Essential reading. A powerful story about justice and morality.", IsVerifiedPurchase = false, HelpfulCount = 20, IsHidden = false, CreatedAt = baseDate.AddDays(-26), UpdatedAt = baseDate.AddDays(-26) },
                new Review { Id = 12, ProductId = 2, UserId = 2, UserName = "User", Rating = 5, ReviewText = "One of the best books I've ever read. Atticus Finch is an incredible character.", IsVerifiedPurchase = false, HelpfulCount = 14, IsHidden = false, CreatedAt = baseDate.AddDays(-21), UpdatedAt = baseDate.AddDays(-21) },
                new Review { Id = 13, ProductId = 2, UserId = 3, UserName = "User2", Rating = 5, ReviewText = "Beautifully written and deeply moving. A must-read classic.", IsVerifiedPurchase = false, HelpfulCount = 16, IsHidden = false, CreatedAt = baseDate.AddDays(-14), UpdatedAt = baseDate.AddDays(-14) },

                // 1984 (Product 3) - Dystopian classic
                new Review { Id = 14, ProductId = 3, UserId = 2, UserName = "User", Rating = 5, ReviewText = "Chilling and relevant. Orwell's vision is more important than ever.", IsVerifiedPurchase = true, HelpfulCount = 18, IsHidden = false, CreatedAt = baseDate.AddDays(-23), UpdatedAt = baseDate.AddDays(-23) },
                new Review { Id = 15, ProductId = 3, UserId = 1, UserName = "admin", Rating = 4, ReviewText = "Thought-provoking and disturbing. A bit depressing but necessary reading.", IsVerifiedPurchase = false, HelpfulCount = 9, IsHidden = false, CreatedAt = baseDate.AddDays(-17), UpdatedAt = baseDate.AddDays(-17) },

                // The Hunger Games (Product 25) - Popular YA
                new Review { Id = 16, ProductId = 25, UserId = 3, UserName = "User2", Rating = 5, ReviewText = "Fast-paced and exciting! Couldn't put it down.", IsVerifiedPurchase = false, HelpfulCount = 13, IsHidden = false, CreatedAt = baseDate.AddDays(-12), UpdatedAt = baseDate.AddDays(-12) },
                new Review { Id = 17, ProductId = 25, UserId = 2, UserName = "User", Rating = 4, ReviewText = "Great dystopian adventure. Katniss is a strong protagonist.", IsVerifiedPurchase = false, HelpfulCount = 8, IsHidden = false, CreatedAt = baseDate.AddDays(-10), UpdatedAt = baseDate.AddDays(-10) },

                // Brave New World (Product 11) - Another dystopian
                new Review { Id = 18, ProductId = 11, UserId = 1, UserName = "admin", Rating = 5, ReviewText = "Brilliant dystopian vision. Huxley was ahead of his time.", IsVerifiedPurchase = false, HelpfulCount = 10, IsHidden = false, CreatedAt = baseDate.AddDays(-13), UpdatedAt = baseDate.AddDays(-13) },
                new Review { Id = 19, ProductId = 11, UserId = 3, UserName = "User2", Rating = 4, ReviewText = "Fascinating world-building. Makes you think about modern society.", IsVerifiedPurchase = false, HelpfulCount = 6, IsHidden = false, CreatedAt = baseDate.AddDays(-8), UpdatedAt = baseDate.AddDays(-8) },

                // Pride and Prejudice (Product 4) - Romance classic
                new Review { Id = 20, ProductId = 4, UserId = 2, UserName = "User", Rating = 5, ReviewText = "Timeless romance with witty dialogue. Elizabeth and Darcy are perfect!", IsVerifiedPurchase = false, HelpfulCount = 17, IsHidden = false, CreatedAt = baseDate.AddDays(-11), UpdatedAt = baseDate.AddDays(-11) },
                new Review { Id = 21, ProductId = 4, UserId = 3, UserName = "User2", Rating = 5, ReviewText = "My favorite Jane Austen novel. The character development is superb.", IsVerifiedPurchase = false, HelpfulCount = 12, IsHidden = false, CreatedAt = baseDate.AddDays(-7), UpdatedAt = baseDate.AddDays(-7) },

                // The Lord of the Rings (Product 16) - Epic fantasy
                new Review { Id = 22, ProductId = 16, UserId = 1, UserName = "admin", Rating = 5, ReviewText = "The pinnacle of fantasy literature. An epic journey in every sense.", IsVerifiedPurchase = false, HelpfulCount = 22, IsHidden = false, CreatedAt = baseDate.AddDays(-9), UpdatedAt = baseDate.AddDays(-9) },
                new Review { Id = 23, ProductId = 16, UserId = 2, UserName = "User", Rating = 4, ReviewText = "Amazing world and story, though quite lengthy. Worth the read!", IsVerifiedPurchase = false, HelpfulCount = 11, IsHidden = false, CreatedAt = baseDate.AddDays(-6), UpdatedAt = baseDate.AddDays(-6) },

                // Lower rated review for variety
                new Review { Id = 24, ProductId = 5, UserId = 3, UserName = "User2", Rating = 3, ReviewText = "Not as engaging as I hoped. The writing style didn't resonate with me.", IsVerifiedPurchase = false, HelpfulCount = 2, IsHidden = false, CreatedAt = baseDate.AddDays(-5), UpdatedAt = baseDate.AddDays(-5) },

                // Recent reviews
                new Review { Id = 25, ProductId = 19, UserId = 2, UserName = "User", Rating = 5, ReviewText = "The best book in the Harry Potter series! The time-turner plot is brilliant.", IsVerifiedPurchase = false, HelpfulCount = 15, IsHidden = false, CreatedAt = baseDate.AddDays(-3), UpdatedAt = baseDate.AddDays(-3) },
                new Review { Id = 26, ProductId = 24, UserId = 1, UserName = "admin", Rating = 5, ReviewText = "Narnia is a magical world. C.S. Lewis created something truly special.", IsVerifiedPurchase = false, HelpfulCount = 14, IsHidden = false, CreatedAt = baseDate.AddDays(-2), UpdatedAt = baseDate.AddDays(-2) },
                new Review { Id = 27, ProductId = 31, UserId = 3, UserName = "User2", Rating = 4, ReviewText = "Interesting take on dystopian society with the faction system.", IsVerifiedPurchase = false, HelpfulCount = 7, IsHidden = false, CreatedAt = baseDate.AddDays(-1), UpdatedAt = baseDate.AddDays(-1) }
            });

            // Seed test payment methods
            PaymentMethods.AddRange(new List<PaymentMethod>
            {
                // Admin user payment methods
                new PaymentMethod { Id = 1, UserId = 1, Type = PaymentMethodType.CreditCard, CardHolderName = "Admin User", CardNumberMasked = "**** **** **** 0000", CardNumberLast4 = "0000", ExpiryMonth = "12", ExpiryYear = "2027", IsDefault = true, CreatedAt = now.AddMonths(-3) },
                new PaymentMethod { Id = 2, UserId = 1, Type = PaymentMethodType.DebitCard, CardHolderName = "Admin User", CardNumberMasked = "**** **** **** 1111", CardNumberLast4 = "1111", ExpiryMonth = "06", ExpiryYear = "2026", IsDefault = false, CreatedAt = now.AddMonths(-2) },
                
                // User (John Doe) payment methods - mix of success/failure cards
                new PaymentMethod { Id = 3, UserId = 2, Type = PaymentMethodType.CreditCard, CardHolderName = "John Doe", CardNumberMasked = "**** **** **** 0000", CardNumberLast4 = "0000", ExpiryMonth = "03", ExpiryYear = "2028", IsDefault = true, CreatedAt = now.AddMonths(-4) },
                new PaymentMethod { Id = 4, UserId = 2, Type = PaymentMethodType.CreditCard, CardHolderName = "John Doe", CardNumberMasked = "**** **** **** 1111", CardNumberLast4 = "1111", ExpiryMonth = "09", ExpiryYear = "2027", IsDefault = false, CreatedAt = now.AddMonths(-3) },
                new PaymentMethod { Id = 5, UserId = 2, Type = PaymentMethodType.PayPal, PayPalEmail = "john.doe@example.com", IsDefault = false, CreatedAt = now.AddMonths(-1) },
                
                // User2 (Jane Smith) payment methods
                new PaymentMethod { Id = 6, UserId = 3, Type = PaymentMethodType.CreditCard, CardHolderName = "Jane Smith", CardNumberMasked = "**** **** **** 2222", CardNumberLast4 = "2222", ExpiryMonth = "11", ExpiryYear = "2026", IsDefault = true, CreatedAt = now.AddMonths(-2) },
                new PaymentMethod { Id = 7, UserId = 3, Type = PaymentMethodType.CreditCard, CardHolderName = "Jane Smith", CardNumberMasked = "**** **** **** 3333", CardNumberLast4 = "3333", ExpiryMonth = "05", ExpiryYear = "2028", IsDefault = false, CreatedAt = now.AddMonths(-1) },
                
                // Additional test cards for various scenarios
                new PaymentMethod { Id = 8, UserId = 2, Type = PaymentMethodType.DebitCard, CardHolderName = "John Doe", CardNumberMasked = "**** **** **** 4444", CardNumberLast4 = "4444", ExpiryMonth = "02", ExpiryYear = "2025", IsDefault = false, CreatedAt = now.AddDays(-30) },
                new PaymentMethod { Id = 9, UserId = 2, Type = PaymentMethodType.CreditCard, CardHolderName = "John Doe", CardNumberMasked = "**** **** **** 6666", CardNumberLast4 = "6666", ExpiryMonth = "08", ExpiryYear = "2029", IsDefault = false, CreatedAt = now.AddDays(-15) }
            });
        }
    }
}