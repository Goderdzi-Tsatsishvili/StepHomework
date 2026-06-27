using FluentAssertions;
using MapsterMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using WebSaklaso.Entities;
using WebSaklaso.Exceptions;
using WebSaklaso.Models.Category;
using WebSaklaso.Models.Common;
using WebSaklaso.Models.Supplier;
using WebSaklaso.Repositories.Contracts;
using WebSaklaso.Service;

namespace WebSaklaso.Tests
{
    public class SupplierServiceTests
    {
        private readonly Mock<ISupplierRepository> _supplierRepoMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly SupplierService _sut;

        private static readonly Guid ValidId = Guid.NewGuid();

        public SupplierServiceTests()
        {
            _sut = new SupplierService(_supplierRepoMock.Object, _mapperMock.Object);
        }

        //Create Method Tests

        [Fact]
        public async Task CreateSupplierAsync_WhenModelIsNull_ThrowsBadRequestException()
        {
            //ARRANGE
            var act = () => _sut.CreateSupplierAsync(null);

            //ACT ASSERT
            await act.Should()
                .ThrowAsync<BadRequestException>()
                .WithMessage("*required*");
        }

        [Fact]
        public async Task CreateSupplierAsync_WhenNameExceeds50Chars_ThrowBadRequestException()
        {
            //ARRANGE
            var model = new SupplierForCreatingDto() { SupplierName = new string('A', 101) };

            //ACT
            var act = () => _sut.CreateSupplierAsync(model);

            //ASSERT
            await act.Should()
                .ThrowAsync<BadRequestException>()
                .WithMessage("*exceed 100*");
        }

        public async Task CreateSupplierAsync_WhenNameIsExactly100Chars_DoesNotThrow()
        {
            //ARRANGE
            var model = new SupplierForCreatingDto() { SupplierName = new string('A', 100) };
            var supplier = new Supplier { Id = ValidId, SupplierName = model.SupplierName };

            _mapperMock.Setup(m => m.Map<Supplier>(model))
                .Returns(supplier);

            _supplierRepoMock.Setup(r => r.AddAsync(supplier))
                .Returns(Task.CompletedTask);

            _supplierRepoMock.Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);


            //ACT
            var act = () => _sut.CreateSupplierAsync(model);


            //ASSERT
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task CreateSupplierAsync_WhenValidModel_CreatesSupplierAndReturnsDto()
        {

            var model = new SupplierForCreatingDto
            {
                SupplierName = "TechCorp Electronics"
            };

            var supplierEntity = new Supplier
            {
                Id = ValidId,
                SupplierName = model.SupplierName
            };

            var resultDto = new SupplierForGettingDto
            {
                SupplierName = "TechCorp Electronics"
            };

            _mapperMock
                .Setup(x => x.Map<Supplier>(model))
                .Returns(supplierEntity);

            _supplierRepoMock
                .Setup(x => x.AddAsync(supplierEntity))
                .Returns(Task.CompletedTask);

            _supplierRepoMock
                .Setup(x => x.SaveAsync())
                .ReturnsAsync(1);

            _mapperMock
                .Setup(x => x.Map<SupplierForGettingDto>(supplierEntity))
                .Returns(resultDto);


            var result = await _sut.CreateSupplierAsync(model);


            _supplierRepoMock.Verify(x => x.AddAsync(supplierEntity), Times.Once);
            _supplierRepoMock.Verify(x => x.SaveAsync(), Times.Once);

            Assert.Equal("TechCorp Electronics", result.SupplierName);

        }

        //Delete Method Tests
        [Fact]
        public async Task DeleteSupplierAsync_WhenGuidIsEmpty_ThrowsBadRequestException()
        {
            var act = () => _sut.DeleteSupplierAsync(Guid.Empty);

            await act.Should()
                .ThrowAsync<BadRequestException>()
                .WithMessage("*required*");
        }

        [Fact]
        public async Task DeleteSupplierAsync_WhenSupplierIsNull_ThrowsNotFoundException()
        {
            var id = ValidId;

            _supplierRepoMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Supplier, bool>>>()))
                .ReturnsAsync((Supplier)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.DeleteSupplierAsync(id));
        }

        [Fact]
        public async Task DeleteSupplierAsync_WhenSupplierExists_DeletesSupplier()
        {
            var id = ValidId;

            var supplier = new Supplier { Id = id };

            _supplierRepoMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Supplier, bool>>>()))
                .ReturnsAsync(supplier);

            await _sut.DeleteSupplierAsync(id);

            _supplierRepoMock.Verify(r => r.Remove(supplier), Times.Once);
            _supplierRepoMock.Verify(r => r.SaveAsync(), Times.Once);
        }

        //GetAll Method Tests
        [Fact]
        public async Task GetSuppliersAsync_WhenSortBySupplierName_UsesSupplierNameSorting()
        {
            var parameters = new PagedRequestDto
            {
                SortBy = "suppliername",
                PageNumber = 1,
                PageSize = 10,
                Ascending = true
            };

            Expression<Func<Supplier, object>> capturedOrderBy = null;

            _supplierRepoMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Supplier, bool>>>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<Expression<Func<Supplier, object>>>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<bool>(),
                    It.IsAny<Expression<Func<Supplier, object>>[]>()))
                .Callback<
                    Expression<Func<Supplier, bool>>,
                    int?,
                    int?,
                    Expression<Func<Supplier, object>>,
                    bool,
                    CancellationToken,
                    bool,
                    Expression<Func<Supplier, object>>[]
                >((filter, pageNumber, pageSize, orderBy, ascending, token, tracking, includes) =>
                {
                    capturedOrderBy = orderBy;
                })
                .ReturnsAsync((
                    new List<Supplier>(),
                    0
                ));



            await _sut.GetAllSuppliersAsync(parameters);



            var supplier = new Supplier
            {
                SupplierName = "TechCorp Electronics",
                Id = ValidId
            };

            var result = capturedOrderBy.Compile()(supplier);

            Assert.Equal("TechCorp Electronics", result);
        }

        [Fact]
        public async Task GetSuppliersAsync_WhenSortByIsInvalid_UsesIdSorting()
        {

            var parameters = new PagedRequestDto
            {
                SortBy = "randomValue",
                PageNumber = 1,
                PageSize = 10,
                Ascending = true
            };

            Expression<Func<Supplier, object>> capturedOrderBy = null;


            _supplierRepoMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Supplier, bool>>>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<Expression<Func<Supplier, object>>>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<bool>(),
                    It.IsAny<Expression<Func<Supplier, object>>[]>()))
                .Callback<
                    Expression<Func<Supplier, bool>>,
                    int?,
                    int?,
                    Expression<Func<Supplier, object>>,
                    bool,
                    CancellationToken,
                    bool,
                    Expression<Func<Supplier, object>>[]
                >((filter, pageNumber, pageSize, orderBy, ascending, token, tracking, includes) =>
                {
                    capturedOrderBy = orderBy;
                })
                .ReturnsAsync((new List<Supplier>(), 0));



            await _sut.GetAllSuppliersAsync(parameters);



            var supplier = new Supplier
            {
                Id = ValidId,
                SupplierName = "TechCorp Electronics"
            };

            var result = capturedOrderBy.Compile()(supplier);


            Assert.Equal(supplier.Id, result);
        }

        [Fact]
        public async Task GetSuppliersAsync_WhenSortByIsNull_UsesIdSorting()
        {
            
            var parameters = new PagedRequestDto
            {
                SortBy = null,
                PageNumber = 1,
                PageSize = 10,
                Ascending = true
            };

            Expression<Func<Supplier, object>> capturedOrderBy = null;


            _supplierRepoMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Supplier, bool>>>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<Expression<Func<Supplier, object>>>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<bool>(),
                    It.IsAny<Expression<Func<Supplier, object>>[]>()))
                .Callback<
                    Expression<Func<Supplier, bool>>,
                    int?,
                    int?,
                    Expression<Func<Supplier, object>>,
                    bool,
                    CancellationToken,
                    bool,
                    Expression<Func<Supplier, object>>[]
                >((filter, pageNumber, pageSize, orderBy, ascending, token, tracking, includes) =>
                {
                    capturedOrderBy = orderBy;
                })
                .ReturnsAsync((new List<Supplier>(), 0));



            await _sut.GetAllSuppliersAsync(parameters);



            var supplier = new Supplier
            {
                Id = ValidId,
                SupplierName = "TechCorp Electronics"
            };

            var result = capturedOrderBy.Compile()(supplier);


            Assert.Equal(supplier.Id, result);
        }

        [Fact]
        public async Task GetSuppliersAsync_PassesCorrectPaginationParametersToRepository()
        {

            var parameters = new PagedRequestDto
            {
                SortBy = "suppliername",
                PageNumber = 3,
                PageSize = 20,
                Ascending = false
            };


            int? capturedPageNumber = null;
            int? capturedPageSize = null;
            bool capturedAscending = true;


            _supplierRepoMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Supplier, bool>>>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<Expression<Func<Supplier, object>>>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<bool>(),
                    It.IsAny<Expression<Func<Supplier, object>>[]>()))
                .Callback<
                    Expression<Func<Supplier, bool>>,
                    int?,
                    int?,
                    Expression<Func<Supplier, object>>,
                    bool,
                    CancellationToken,
                    bool,
                    Expression<Func<Supplier, object>>[]
                >((filter, pageNumber, pageSize, orderBy, ascending, token, tracking, includes) =>
                {
                    capturedPageNumber = pageNumber;
                    capturedPageSize = pageSize;
                    capturedAscending = ascending;
                })
                .ReturnsAsync((new List<Supplier>(), 0));



            await _sut.GetAllSuppliersAsync(parameters);



            Assert.Equal(3, capturedPageNumber);
            Assert.Equal(20, capturedPageSize);
            Assert.False(capturedAscending);
        }

        [Fact]
        public async Task GetSuppliersAsync_WhenRepositoryReturnsEmptyList_ReturnsEmptyResponse()
        {

            var parameters = new PagedRequestDto
            {
                PageNumber = 1,
                PageSize = 10
            };


            _supplierRepoMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Supplier, bool>>>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<Expression<Func<Supplier, object>>>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<bool>(),
                    It.IsAny<Expression<Func<Supplier, object>>[]>()))
                .ReturnsAsync((new List<Supplier>(), 0));



            var result = await _sut.GetAllSuppliersAsync(parameters);



            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public async Task GetSuppliersAsync_WhenSuppliersExist_ReturnsMappedDtos()
        {

            var parameters = new PagedRequestDto
            {
                PageNumber = 1,
                PageSize = 10
            };


            var suppliers = new List<Supplier>
            {
                new Supplier
                {
                    Id = ValidId,
                    SupplierName = "TechCorp Electronics"
                }
            };


            var dtos = new List<SupplierForGettingDto>
            {
                 new SupplierForGettingDto
                {
                    SupplierName = "TechCorp Electronics"
                }
            };


            _supplierRepoMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Supplier, bool>>>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<Expression<Func<Supplier, object>>>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<bool>(),
                    It.IsAny<Expression<Func<Supplier, object>>[]>()))
                .ReturnsAsync((suppliers, 1));


            _mapperMock
                .Setup(x => x.Map<IEnumerable<SupplierForGettingDto>>(suppliers))
                .Returns(dtos);



            var result = await _sut.GetAllSuppliersAsync(parameters);



            Assert.Single(result.Items);
            Assert.Equal("TechCorp Electronics", result.Items.First().SupplierName);
            Assert.Equal(1, result.TotalCount);
        }

        //GetById Method Tests

        [Fact]
        public async Task GetSupplierById_WhenGuidIsEmpty_ThrowsBadRequestException()
        {
            var act = () => _sut.GetSupplierByIdAsync(Guid.Empty);

            await act.Should()
                .ThrowAsync<BadRequestException>()
                .WithMessage("*required*");
        }
        [Fact]
        public async Task GetSupplierById_WhenSupplierIsNull_ThrowsNotFoundException()
        {
            var id = ValidId;

            _supplierRepoMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Supplier, bool>>>()))
                .ReturnsAsync((Supplier)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.GetSupplierByIdAsync(id));
        }

        [Fact]
        public async Task GetSupplierById_WhenSupplierExists_ReturnsMappedDto()
        {
            var id = ValidId;

            var supplier = new Supplier
            {
                Id = id,
                SupplierName = "TechCorp Electronics"
            };

            var dto = new SupplierForGettingDto
            {
                SupplierName = "TechCorp Electronics"
            };

            _supplierRepoMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<Supplier, bool>>>()))
                .ReturnsAsync(supplier);

            _mapperMock.Setup(x => x.Map<SupplierForGettingDto>(supplier))
                .Returns(dto);

            var result = await _sut.GetSupplierByIdAsync(id);

            Assert.NotNull(result);
            Assert.Equal("TechCorp Electronics", result.SupplierName);
        }

        //Update Method Tests
        [Fact]
        public async Task UpdateSupplierAsync_WhenModelIsNull_ThrowsBadRequestException()
        {
            await Assert.ThrowsAsync<BadRequestException>(() =>
                _sut.UpdateSupplierAsync(null));
        }

        [Fact]
        public async Task UpdateSupplierAsync_WhenIdIsEmpty_ThrowsBadRequestException()
        {
            var model = new SupplierForUpdatingDto
            {
                Id = Guid.Empty,
                SupplierName = "ValidName"
            };

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _sut.UpdateSupplierAsync(model));
        }

        [Fact]
        public async Task UpdateSupplierAsync_WhenNameTooLong_ThrowsBadRequestException()
        {
            var model = new SupplierForUpdatingDto
            {
                Id = ValidId,
                SupplierName = new string('A', 101)
            };

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _sut.UpdateSupplierAsync(model));
        }

        [Fact]
        public async Task UpdateSupplierAsync_WhenSupplierNotFound_ThrowsNotFoundException()
        {
            var model = new SupplierForUpdatingDto
            {
                Id = ValidId,
                SupplierName = "ValidName"
            };

            _supplierRepoMock
                .Setup(x => x.GetAsync(It.IsAny<Expression<Func<Supplier, bool>>>()))
                .ReturnsAsync((Supplier)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _sut.UpdateSupplierAsync(model));
        }

        [Fact]
        public async Task UpdateSupplierAsync_WhenValidRequest_UpdatesAndReturnsMappedDto()
        {

            var model = new SupplierForUpdatingDto
            {
                Id = ValidId,
                SupplierName = "UpdatedName"
            };

            var supplier = new Supplier
            {
                Id = model.Id,
                SupplierName = "OldName"
            };

            var resultDto = new SupplierForGettingDto
            {
                SupplierName = "UpdatedName"
            };

            _supplierRepoMock
                .Setup(x => x.GetAsync(It.IsAny<Expression<Func<Supplier, bool>>>()))
                .ReturnsAsync(supplier);

            _mapperMock
                .Setup(x => x.Map(model, supplier));

            _mapperMock
                .Setup(x => x.Map<SupplierForGettingDto>(supplier))
                .Returns(resultDto);


            var result = await _sut.UpdateSupplierAsync(model);


            _supplierRepoMock.Verify(x => x.Update(supplier), Times.Once);
            _supplierRepoMock.Verify(x => x.SaveAsync(), Times.Once);

            Assert.Equal("UpdatedName", result.SupplierName);
        }
    }
}