﻿using Business.Abstract;
using Business.Constants;
using Business.ValidationsRules.FluentValidation;
using Core.Aspects.Autofac.Validation;
using Core.Entities.Business;
using Core.Entities.Utilities.Helpers.FileHelper;
using Core.Entities.Utilities.Results;
using Core.Utilities.Helpers.FileHelper;
using DataAccess.Abstract;
using Entities.Concrete;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class CarImageManager : ICarImagesService
    {
        ICarImagesDal _carImageDal;
        IFileHelper _fileHelper;

        public CarImageManager(IFileHelper fileHelper)
        {
            _fileHelper = fileHelper;
        }

        public CarImageManager(ICarImagesDal carImageDal)
        {
            _carImageDal = carImageDal;
        }
        public IResult Add(IFormFile file, CarImages carImage)
        {
            IResult result = BusinessRules.Run(CheckIfCarImageLimit(carImage.CarId));
            if (result != null)
            {
                return result;
            }
            carImage.ImagePath = _fileHelper.Upload(file, PathConstants.ImagesPath);
            carImage.Date = DateTime.Now;
            _carImageDal.Add(carImage);
            return new SuccessResult("Resim başarıyla yüklendi");
        }

        public IResult Delete(CarImages carImage)
        {
            _fileHelper.Delete(PathConstants.ImagesPath + carImage.ImagePath);
            _carImageDal.Delete(carImage);
            return new SuccessResult();
        }
        public IResult Update(IFormFile file, CarImages carImage)
        {
            carImage.ImagePath = _fileHelper.Update(file, PathConstants.ImagesPath + carImage.ImagePath, PathConstants.ImagesPath);
            _carImageDal.Update(carImage);
            return new SuccessResult();
        }

        public IDataResult<List<CarImages>> GetByCarId(int carId)
        {
            var result = BusinessRules.Run(CheckCarImage(carId));
            if (result != null)
            {
                return new ErrorDataResult<List<CarImages>>(GetDefaultImage(carId).Data);
            }
            return new SuccessDataResult<List<CarImages>>(_carImageDal.GetAll(c => c.CarId == carId));
        }

        public IDataResult<CarImages> GetByImageId(int imageId)
        {
            return new SuccessDataResult<CarImages>(_carImageDal.Get(c => c.Id == imageId));
        }

        public IDataResult<List<CarImages>> GetAll()
        {
            return new SuccessDataResult<List<CarImages>>(_carImageDal.GetAll());
        }
        private IResult CheckIfCarImageLimit(int carId)
        {
            var result = _carImageDal.GetAll(c => c.CarId == carId).Count;
            if (result >= 5)
            {
                return new ErrorResult();
            }
            return new SuccessResult();
        }
        private IDataResult<List<CarImages>> GetDefaultImage(int carId)
        {

            List<CarImages> carImage = new List<CarImages>();
            carImage.Add(new CarImages { CarId = carId, Date = DateTime.Now, ImagePath = "DefaultImage.jpg" });
            return new SuccessDataResult<List<CarImages>>(carImage);
        }
        private IResult CheckCarImage(int carId)
        {
            var result = _carImageDal.GetAll(c => c.CarId == carId).Count;
            if (result > 0)
            {
                return new SuccessResult();
            }
            return new ErrorResult();
        }
    }
}