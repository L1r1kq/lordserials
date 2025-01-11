using Moq;
using System.Data.SqlClient;
using Xunit;
using MyORMLibrary;
using MyLordSerialsLibrary;

namespace MyORMTest
{
    public class ORMContextTests
    {
        private readonly Mock<SqlConnection> _mockConnection;
        private readonly Mock<SqlCommand> _mockCommand;
        private readonly ORMContext<User> _context;

        public ORMContextTests()
        {
            _mockConnection = new Mock<SqlConnection>("Your_Connection_String");
            _mockCommand = new Mock<SqlCommand>();
            _context = new ORMContext<User>("Your_Connection_String", "Users");
        }

        [Fact]
        public void Create_ShouldExecuteInsertCommand()
        {
            // Arrange
            var user = new User { Id = 1, Name = "John Doe", Email = "john@example.com" };

            _mockCommand.Setup(m => m.ExecuteNonQuery()).Returns(1);

            // Act
            _context.Create(user);

            // Assert
            _mockCommand.Verify(m => m.ExecuteNonQuery(), Times.Once);
        }

        [Fact]
        public void ReadById_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var userId = 1;
            var expectedUser = new User { Id = 1, Name = "John Doe", Email = "john@example.com" };

            _mockCommand.Setup(m => m.ExecuteReader()).Returns(MockReader(expectedUser));

            // Act
            var result = _context.ReadById(userId);

            // Assert
            Assert.Equal(expectedUser.Id, result.Id);
            Assert.Equal(expectedUser.Name, result.Name);
        }

        [Fact]
        public void Update_ShouldExecuteUpdateCommand()
        {
            // Arrange
            var user = new User { Id = 1, Name = "John Doe", Email = "john@example.com" };

            _mockCommand.Setup(m => m.ExecuteNonQuery()).Returns(1);

            // Act
            _context.Update(user);

            // Assert
            _mockCommand.Verify(m => m.ExecuteNonQuery(), Times.Once);
        }

        [Fact]
        public void Delete_ShouldExecuteDeleteCommand()
        {
            // Arrange
            var user = new User { Id = 1, Name = "John Doe", Email = "john@example.com" };

            _mockCommand.Setup(m => m.ExecuteNonQuery()).Returns(1);

            // Act
            _context.DeleteUser(user);

            // Assert
            _mockCommand.Verify(m => m.ExecuteNonQuery(), Times.Once);
        }

        private SqlDataReader MockReader(User user)
        {
            var dataReaderMock = new Mock<SqlDataReader>();
            dataReaderMock.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);
            dataReaderMock.Setup(r => r.GetName(It.IsAny<int>())).Returns("Id");
            dataReaderMock.Setup(r => r.GetValue(It.IsAny<int>())).Returns(user.Id);
            return dataReaderMock.Object;
        }
    }
}