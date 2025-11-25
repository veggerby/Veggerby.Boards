using Veggerby.Boards.Monopoly;

namespace Veggerby.Boards.Tests.Monopoly;

public class MonopolyBoardConfigurationTests
{
    [Fact]
    public void Squares_Has40Entries()
    {
        // arrange

        // act
        var squares = MonopolyBoardConfiguration.Squares;

        // assert
        squares.Count.Should().Be(40);
    }

    [Fact]
    public void GetSquare_Position0_ReturnsGo()
    {
        // arrange

        // act
        var square = MonopolyBoardConfiguration.GetSquare(0);

        // assert
        square.Name.Should().Be("Go");
        square.SquareType.Should().Be(SquareType.Go);
    }

    [Fact]
    public void GetSquare_Position10_ReturnsJail()
    {
        // arrange

        // act
        var square = MonopolyBoardConfiguration.GetSquare(10);

        // assert
        square.Name.Should().Be("Jail");
        square.SquareType.Should().Be(SquareType.Jail);
    }

    [Fact]
    public void GetSquare_Position30_ReturnsGoToJail()
    {
        // arrange

        // act
        var square = MonopolyBoardConfiguration.GetSquare(30);

        // assert
        square.Name.Should().Be("Go To Jail");
        square.SquareType.Should().Be(SquareType.GoToJail);
    }

    [Fact]
    public void GetSquare_Position20_ReturnsFreeParking()
    {
        // arrange

        // act
        var square = MonopolyBoardConfiguration.GetSquare(20);

        // assert
        square.Name.Should().Be("Free Parking");
        square.SquareType.Should().Be(SquareType.FreeParking);
    }

    [Theory]
    [InlineData(1, "Mediterranean Avenue", PropertyColorGroup.Brown, 60)]
    [InlineData(3, "Baltic Avenue", PropertyColorGroup.Brown, 60)]
    [InlineData(39, "Boardwalk", PropertyColorGroup.DarkBlue, 400)]
    [InlineData(37, "Park Place", PropertyColorGroup.DarkBlue, 350)]
    public void GetSquare_Properties_HaveCorrectData(int position, string name, PropertyColorGroup color, int price)
    {
        // arrange

        // act
        var square = MonopolyBoardConfiguration.GetSquare(position);

        // assert
        square.Name.Should().Be(name);
        square.ColorGroup.Should().Be(color);
        square.Price.Should().Be(price);
        square.SquareType.Should().Be(SquareType.Property);
    }

    [Theory]
    [InlineData(5, "Reading Railroad")]
    [InlineData(15, "Pennsylvania Railroad")]
    [InlineData(25, "B&O Railroad")]
    [InlineData(35, "Short Line Railroad")]
    public void GetSquare_Railroads_HaveCorrectData(int position, string name)
    {
        // arrange

        // act
        var square = MonopolyBoardConfiguration.GetSquare(position);

        // assert
        square.Name.Should().Be(name);
        square.SquareType.Should().Be(SquareType.Railroad);
        square.ColorGroup.Should().Be(PropertyColorGroup.Railroad);
        square.Price.Should().Be(200);
    }

    [Theory]
    [InlineData(12, "Electric Company")]
    [InlineData(28, "Water Works")]
    public void GetSquare_Utilities_HaveCorrectData(int position, string name)
    {
        // arrange

        // act
        var square = MonopolyBoardConfiguration.GetSquare(position);

        // assert
        square.Name.Should().Be(name);
        square.SquareType.Should().Be(SquareType.Utility);
        square.ColorGroup.Should().Be(PropertyColorGroup.Utility);
        square.Price.Should().Be(150);
    }

    [Fact]
    public void GetColorGroupCount_Brown_Returns2()
    {
        // arrange

        // act
        var count = MonopolyBoardConfiguration.GetColorGroupCount(PropertyColorGroup.Brown);

        // assert
        count.Should().Be(2);
    }

    [Fact]
    public void GetColorGroupCount_Railroad_Returns4()
    {
        // arrange

        // act
        var count = MonopolyBoardConfiguration.GetColorGroupCount(PropertyColorGroup.Railroad);

        // assert
        count.Should().Be(4);
    }

    [Fact]
    public void GetColorGroupCount_DarkBlue_Returns2()
    {
        // arrange

        // act
        var count = MonopolyBoardConfiguration.GetColorGroupCount(PropertyColorGroup.DarkBlue);

        // assert
        count.Should().Be(2);
    }

    [Fact]
    public void GetTileId_ReturnsCorrectFormat()
    {
        // arrange

        // act
        var tileId = MonopolyBoardConfiguration.GetTileId(5);

        // assert
        tileId.Should().Be("square-5");
    }

    [Fact]
    public void GetPosition_ReturnsCorrectPosition()
    {
        // arrange

        // act
        var position = MonopolyBoardConfiguration.GetPosition("square-15");

        // assert
        position.Should().Be(15);
    }
}
