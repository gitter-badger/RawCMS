﻿//******************************************************************************
// <copyright file="license.md" company="RawCMS project  (https://github.com/arduosoft/RawCMS)">
// Copyright (c) 2019 RawCMS project  (https://github.com/arduosoft/RawCMS)
// RawCMS project is released under GPL3 terms, see LICENSE file on repository root at  https://github.com/arduosoft/RawCMS .
// </copyright>
// <author>Daniele Fontani, Emanuele Bucarelli, Francesco Min�</author>
// <autogenerated>true</autogenerated>
//******************************************************************************
using Newtonsoft.Json.Linq;
using RawCMS.Library.Core;
using RawCMS.Library.Core.Interfaces;
using RawCMS.Library.Schema;
using RawCMS.Library.Service;
using System.Collections.Generic;
using System.Linq;

namespace RawCMS.Library.Lambdas
{
    public class EntityValidation : SchemaValidationLambda, IRequireCrudService, IInitable, IRequireApp
    {
        public override string Name => "Entity Validation";

        public override string Description => "Provide generic entity validation, based on configuration";

        private static Dictionary<string, CollectionSchema> entities = new Dictionary<string, CollectionSchema>();
        private static List<FieldTypeValidator> typeValidators = new List<FieldTypeValidator>();

        private CRUDService service;

        public EntityValidation()
        {
        }

        public void Init()
        {
            InitSchema();
            InitValidators();
        }

        private void InitValidators()
        {
            typeValidators = manager.GetAssignablesInstances<FieldTypeValidator>();
        }

        private void InitSchema()
        {
            JArray dbEntities = service.Query("_schema", new DataModel.DataQuery()
            {
                PageNumber = 1,
                PageSize = int.MaxValue,
                RawQuery = null
            }).Items;

            foreach (JToken item in dbEntities)
            {
                CollectionSchema schema = item.ToObject<CollectionSchema>();
                if (schema.CollectionName != null && !string.IsNullOrEmpty(schema.CollectionName.ToString()))
                {
                    var haveIdField = schema.FieldSettings.Where(x => x.Name.Equals("_id")).Count() > 0;
                    if (!haveIdField)
                    {
                        var field = new Field();
                        field.Name = "_id";
                        field.BaseType = FieldBaseType.String;
                        field.Type = "ObjectId";
                        field.Required = true;
                        schema.FieldSettings.Add(field);
                    }

                    entities[schema.CollectionName] = schema;
                }
            }
        }

        public override List<Error> Validate(JObject input, string collection)
        {
            List<Error> errors = new List<Error>();
            if (entities.TryGetValue(collection, out CollectionSchema settings))
            {
                //do validation!

                if (!settings.AllowNonMappedFields)
                {
                    foreach (JProperty field in input.Properties())
                    {
                        if (!settings.FieldSettings.Any(x => x.Name == field.Name))
                        {
                            errors.Add(new Error()
                            {
                                Code = "Forbidden Field",
                                Title = $"Field {field.Name} not in allowed field list",
                            });
                        }
                    }
                }

                foreach (Field field in settings.FieldSettings)
                {
                    errors.AddRange(ValidateField(field, input, collection));
                }
            }

            return errors;
        }

        private IEnumerable<Error> ValidateField(Field field, JObject input, string collection)
        {
            List<Error> errors = new List<Error>();

            if (field.Required && input[field.Name] == null)
            {
                errors.Add(new Error()
                {
                    Code = "REQUIRED",
                    Title = "Field " + field.Name + " is required"
                });
            }

            FieldTypeValidator typeValidator = typeValidators.FirstOrDefault(x => x.Type == field.Type);
            if (typeValidator != null)
            {
                errors.AddRange(typeValidator.Validate(input, field));
            }
            return errors;
        }

        public Dictionary<string,CollectionSchema> GetCollections()
        {
            return entities;
        }

        public void SetCRUDService(CRUDService service)
        {
            this.service = service;
        }

        private AppEngine manager;

        public void SetAppEngine(AppEngine manager)
        {
            this.manager = manager;
        }
    }
}