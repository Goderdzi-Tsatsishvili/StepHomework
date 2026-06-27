using FluentAssertions;
using MapsterMapper;
using Moq;
using System.Linq.Expressions;
using WebSaklaso.Entities;
using WebSaklaso.Exceptions;
using WebSaklaso.Models.Category;
using WebSaklaso.Models.Common;
using WebSaklaso.Repositories.Contracts;
using WebSaklaso.Service;

namespace WebSaklaso.Tests
{
    public class CategoryServiceTests
    {
        //Arrange Act Assert  მომზადება მოქმედება მტკიცება

        private readonly Mock<ICategoryRepository> _categoryRepoMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly CategoryService _sut;

        private static readonly Guid ValidId = Guid.NewGuid();

        public CategoryServiceTests()
        {
            _sut = new CategoryService(_categoryRepoMock.Object, _mapperMock.Object);
        }

        //Create Method Tests

        [Fact]
        public async Task CreateCategoryAsync_WhenModelIsNull_ThrowsBadRequestException()
        {
            //ARRANGE
            var act = () => _sut.CreateCategoryAsync(null);

            //ACT ASSERT
            await act.Should()
                .ThrowAsync<BadRequestException>()
                .WithMessage("*required*");
        }

        [Fact]
        public async Task CreateCategoryAsync_WhenNameExceeds50Chars_ThrowBadRequestException()
        {
            //ARRANGE
            var model = new CategoryForCreatingDto() { CategoryName = new string('A', 51) };

            //ACT
            var act = () => _sut.CreateCategoryAsync(model);

            //ASSERT
            await act.Should()
                .ThrowAsync<BadRequestException>()
                .WithMessage("*exceed 50*");
        }

        public async Task CreateCategoryAsync_WhenNameIsExactly50Chars_DoesNotThrow()
        {
            //ARRANGE
            var model = new CategoryForCreatingDto() { CategoryName = new string('A', 50) };
            var category = new Category { Id = ValidId, CategoryName = model.CategoryName };

            _mapperMock.Setup(m => m.Map<Category>(model))
                .Returns(category);

            _categoryRepoMock.Setup(r => r.AddAsync(category))
                .Returns(Task.CompletedTask);

            _categoryRepoMock.Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);


            //ACT
            var act = () => _sut.CreateCategoryAsync(model);


            //ASSERT
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task CreateCategoryAsync_WhenValid_SavesAndReturnsId()
        {
            //ARRANGE
            var model = new CategoryForCreatingDto() { CategoryName = "Electronics" };
            var category = new Category { Id = ValidId, CategoryName = "Electronics" };

            _mapperMock.Setup(m => m.Map<Category>(model))
                .Returns(category);

            _categoryRepoMock.Setup(r => r.AddAsync(category))
                .Returns(Task.CompletedTask);

            _categoryRepoMock.Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);


            //ACT
            var result = await _sut.CreateCategoryAsync(model);


            //ASSERT
            result.Should().Be(ValidId);
            _categoryRepoMock.Verify(r => r.AddAsync(category), Times.Once);
            _categoryRepoMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        //Delete Method Tests

        [Fact]
        public async Task DeleteCategoryAsync_WhenGuidIsEmpty_ThrowsBadRequestException()
        {
            var act = () => _sut.DeleteCategoryAsync(Guid.Empty);

            await act.Should()
                .ThrowAsync<BadRequestException>()
                .WithMessage("*required*");
        }

        [Fact]
        public async Task DeleteCategoryAsync_WhenCategoryIsNull_ThrowsNotFoundException()
        {
            var id = ValidId;

            _categoryRepoMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync((Category)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.DeleteCategoryAsync(id)); 
        }

        [Fact]
        public async Task DeleteCategoryAsync_WhenCategoryExists_DeletesCategory()
        {
            var id = ValidId;

            var category = new Category { Id = id };

            _categoryRepoMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(category);

            await _sut.DeleteCategoryAsync(id);

            _categoryRepoMock.Verify(r => r.Remove(category), Times.Once);
            _categoryRepoMock.Verify(r => r.SaveAsync(), Times.Once);
        }

        //GetAll Method Tests
        [Fact]
        public async Task GetCategoriesAsync_WhenSortByCategoryName_UsesCategoryNameSorting()
        {
            var parameters = new PagedRequestDto
            {
                SortBy = "categoryname",
                PageNumber = 1,
                PageSize = 10,
                Ascending = true
            };

            Expression<Func<Category, object>> capturedOrderBy = null;

            _categoryRepoMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Category, bool>>>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<Expression<Func<Category, object>>>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<bool>(),
                    It.IsAny<Expression<Func<Category, object>>[]>()))
                .Callback<
                    Expression<Func<Category, bool>>,
                    int?,
                    int?,
                    Expression<Func<Category, object>>,
                    bool,
                    CancellationToken,
                    bool,
                    Expression<Func<Category, object>>[]
                >((filter, pageNumber, pageSize, orderBy, ascending, token, tracking, includes) =>
                {
                    capturedOrderBy = orderBy;
                })
                .ReturnsAsync((
                    new List<Category>(),
                    0
                ));


            
            await _sut.GetAllCategoriesAsync(parameters);


            
            var category = new Category
            {
                CategoryName = "Electronics",
                Id = ValidId
            };

            var result = capturedOrderBy.Compile()(category);

            Assert.Equal("Electronics", result);
        }

        [Fact]
        public async Task GetCategoriesAsync_WhenSortByIsInvalid_UsesIdSorting()
        {
            
            var parameters = new PagedRequestDto
            {
                SortBy = "randomValue",
                PageNumber = 1,
                PageSize = 10,
                Ascending = true
            };

            Expression<Func<Category, object>> capturedOrderBy = null;


            _categoryRepoMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Category, bool>>>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<Expression<Func<Category, object>>>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<bool>(),
                    It.IsAny<Expression<Func<Category, object>>[]>()))
                .Callback<
                    Expression<Func<Category, bool>>,
                    int?,
                    int?,
                    Expression<Func<Category, object>>,
                    bool,
                    CancellationToken,
                    bool,
                    Expression<Func<Category, object>>[]
                >((filter, pageNumber, pageSize, orderBy, ascending, token, tracking, includes) =>
                {
                    capturedOrderBy = orderBy;
                })
                .ReturnsAsync((new List<Category>(), 0));


            
            await _sut.GetAllCategoriesAsync(parameters);


            
            var category = new Category
            {
                Id = ValidId,
                CategoryName = "Electronics"
            };

            var result = capturedOrderBy.Compile()(category);


            Assert.Equal(category.Id, result);
        }

        [Fact]
        public async Task GetCategoriesAsync_WhenSortByIsNull_UsesIdSorting()
        {
            
            var parameters = new PagedRequestDto
            {
                SortBy = null,
                PageNumber = 1,
                PageSize = 10,
                Ascending = true
            };

            Expression<Func<Category, object>> capturedOrderBy = null;


            _categoryRepoMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Category, bool>>>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<Expression<Func<Category, object>>>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<bool>(),
                    It.IsAny<Expression<Func<Category, object>>[]>()))
                .Callback<
                    Expression<Func<Category, bool>>,
                    int?,
                    int?,
                    Expression<Func<Category, object>>,
                    bool,
                    CancellationToken,
                    bool,
                    Expression<Func<Category, object>>[]
                >((filter, pageNumber, pageSize, orderBy, ascending, token, tracking, includes) =>
                {
                    capturedOrderBy = orderBy;
                })
                .ReturnsAsync((new List<Category>(), 0));


            
            await _sut.GetAllCategoriesAsync(parameters);


            
            var category = new Category
            {
                Id = ValidId,
                CategoryName = "Electronics"
            };

            var result = capturedOrderBy.Compile()(category);


            Assert.Equal(category.Id, result);
        }

        [Fact]
        public async Task GetCategoriesAsync_PassesCorrectPaginationParametersToRepository()
        {
            
            var parameters = new PagedRequestDto
            {
                SortBy = "categoryname",
                PageNumber = 3,
                PageSize = 20,
                Ascending = false
            };


            int? capturedPageNumber = null;
            int? capturedPageSize = null;
            bool capturedAscending = true;


            _categoryRepoMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Category, bool>>>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<Expression<Func<Category, object>>>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<bool>(),
                    It.IsAny<Expression<Func<Category, object>>[]>()))
                .Callback<
                    Expression<Func<Category, bool>>,
                    int?,
                    int?,
                    Expression<Func<Category, object>>,
                    bool,
                    CancellationToken,
                    bool,
                    Expression<Func<Category, object>>[]
                >((filter, pageNumber, pageSize, orderBy, ascending, token, tracking, includes) =>
                {
                    capturedPageNumber = pageNumber;
                    capturedPageSize = pageSize;
                    capturedAscending = ascending;
                })
                .ReturnsAsync((new List<Category>(), 0));


            
            await _sut.GetAllCategoriesAsync(parameters);


            
            Assert.Equal(3, capturedPageNumber);
            Assert.Equal(20, capturedPageSize);
            Assert.False(capturedAscending);
        }

        [Fact]
        public async Task GetCategoriesAsync_WhenRepositoryReturnsEmptyList_ReturnsEmptyResponse()
        {
            
            var parameters = new PagedRequestDto
            {
                PageNumber = 1,
                PageSize = 10
            };


            _categoryRepoMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Category, bool>>>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<Expression<Func<Category, object>>>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<bool>(),
                    It.IsAny<Expression<Func<Category, object>>[]>()))
                .ReturnsAsync((new List<Category>(), 0));


            
            var result = await _sut.GetAllCategoriesAsync(parameters);


            
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public async Task GetCategoriesAsync_WhenCategoriesExist_ReturnsMappedDtos()
        {
            
            var parameters = new PagedRequestDto
            {
                PageNumber = 1,
                PageSize = 10
            };


            var categories = new List<Category>
            {
                new Category
                {
                    Id = ValidId,
                    CategoryName = "Electronics"
                }
            };


            var dtos = new List<CategoryForGettingDto>
            {
                 new CategoryForGettingDto
                {
                    CategoryName = "Electronics"
                }
            };


            _categoryRepoMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Category, bool>>>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<Expression<Func<Category, object>>>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<bool>(),
                    It.IsAny<Expression<Func<Category, object>>[]>()))
                .ReturnsAsync((categories, 1));


            _mapperMock
                .Setup(x => x.Map<IEnumerable<CategoryForGettingDto>>(categories))
                .Returns(dtos);


            
            var result = await _sut.GetAllCategoriesAsync(parameters);


            
            Assert.Single(result.Items);
            Assert.Equal("Electronics", result.Items.First().CategoryName);
            Assert.Equal(1, result.TotalCount);
        }

        //GetById Method Tests

        [Fact]
        public async Task GetCategoryById_WhenGuidIsEmpty_ThrowsBadRequestException()
        {
            var act = () => _sut.GetCategoryByIdAsync(Guid.Empty);

            await act.Should()
                .ThrowAsync<BadRequestException>()
            .WithMessage("*required*");
        }
        [Fact]
        public async Task GetCategoryById_WhenCategoryIsNull_ThrowsNotFoundException()
        {
            var id = ValidId;

            _categoryRepoMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync((Category)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.GetCategoryByIdAsync(id));
        }

        [Fact]
        public async Task GetCategoryById_WhenCategoryExists_ReturnsMappedDto()
        {
            var id = ValidId;

            var category = new Category
            {
                Id = id,
                CategoryName = "Electronics"
            };

            var dto = new CategoryForGettingDto
            {
                CategoryName = "Electronics"
            };

            _categoryRepoMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(category);

            _mapperMock.Setup(x => x.Map<CategoryForGettingDto>(category))
                .Returns(dto);

            var result = await _sut.GetCategoryByIdAsync(id);

            Assert.NotNull(result);
            Assert.Equal("Electronics", result.CategoryName);
        }

        //Update Method Tests
        [Fact]
        public async Task UpdateCategoryAsync_WhenModelIsNull_ThrowsBadRequestException()
        {
            await Assert.ThrowsAsync<BadRequestException>(() =>
                _sut.UpdateCategoryAsync(null));
        }

        [Fact]
        public async Task UpdateCategoryAsync_WhenIdIsEmpty_ThrowsBadRequestException()
        {
            var model = new CategoryForUpdatingDto
            {
                Id = Guid.Empty,
                CategoryName = "ValidName"
            };

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _sut.UpdateCategoryAsync(model));
        }

        [Fact]
        public async Task UpdateCategoryAsync_WhenNameTooLong_ThrowsBadRequestException()
        {
            var model = new CategoryForUpdatingDto
            {
                Id = ValidId,
                CategoryName = new string('A', 51)
            };

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _sut.UpdateCategoryAsync(model));
        }

        [Fact]
        public async Task UpdateCategoryAsync_WhenCategoryNotFound_ThrowsNotFoundException()
        {
            var model = new CategoryForUpdatingDto
            {
                Id = ValidId,
                CategoryName = "ValidName"
            };

            _categoryRepoMock
                .Setup(x => x.GetAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync((Category)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _sut.UpdateCategoryAsync(model));
        }

        [Fact]
        public async Task UpdateCategoryAsync_WhenValidRequest_UpdatesAndReturnsMappedDto()
        {
            
            var model = new CategoryForUpdatingDto
            {
                Id = ValidId,
                CategoryName = "UpdatedName"
            };

            var category = new Category
            {
                Id = model.Id,
                CategoryName = "OldName"
            };

            var resultDto = new CategoryForGettingDto
            {
                CategoryName = "UpdatedName"
            };

            _categoryRepoMock
                .Setup(x => x.GetAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(category);

            _mapperMock
                .Setup(x => x.Map(model, category));

            _mapperMock
                .Setup(x => x.Map<CategoryForGettingDto>(category))
                .Returns(resultDto);

            
            var result = await _sut.UpdateCategoryAsync(model);

            
            _categoryRepoMock.Verify(x => x.Update(category), Times.Once);
            _categoryRepoMock.Verify(x => x.SaveAsync(), Times.Once);

            Assert.Equal("UpdatedName", result.CategoryName);
        }
    }
}