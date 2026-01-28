// MongoDB initialization script for CAP.BSP.DSP
// This script creates the database, collections, and seed data

// Switch to database (creates if doesn't exist)
db = db.getSiblingDB('cap_bsp_dsp');

// Create collections with validation
db.createCollection('declarationReadModel', {
  validator: {
    $jsonSchema: {
      bsonType: 'object',
      required: ['declarationId', 'numeroContrat', 'typeSinistre', 'dateSinistre', 'statut', 'dateDeclaration', 'version'],
      properties: {
        declarationId: {
          bsonType: 'string',
          description: 'Unique declaration identifier (ULID)'
        },
        numeroContrat: {
          bsonType: 'string',
          description: 'Contract number (format: POL-YYYYMMDD-XXXXX)'
        },
        typeSinistre: {
          enum: ['ACCIDENT_CORPOREL', 'DEGATS_MATERIELS', 'RESPONSABILITE_CIVILE'],
          description: 'Claim type code'
        },
        dateSinistre: {
          bsonType: 'date',
          description: 'Date when the claim occurred'
        },
        lieuSinistre: {
          bsonType: 'string',
          description: 'Location where the claim occurred'
        },
        description: {
          bsonType: 'string',
          description: 'Claim description'
        },
        statut: {
          enum: ['DECLAREE', 'EN_COURS', 'CLOTUREE', 'REJETEE'],
          description: 'Current claim status'
        },
        dateDeclaration: {
          bsonType: 'date',
          description: 'Declaration timestamp'
        },
        version: {
          bsonType: 'long',
          description: 'Event version for optimistic concurrency'
        }
      }
    }
  }
});

db.createCollection('typeSinistreReference', {
  validator: {
    $jsonSchema: {
      bsonType: 'object',
      required: ['code', 'libelle', 'actif'],
      properties: {
        code: {
          enum: ['ACCIDENT_CORPOREL', 'DEGATS_MATERIELS', 'RESPONSABILITE_CIVILE'],
          description: 'Claim type code'
        },
        libelle: {
          bsonType: 'string',
          description: 'Claim type label'
        },
        description: {
          bsonType: 'string',
          description: 'Detailed description'
        },
        actif: {
          bsonType: 'bool',
          description: 'Is this claim type active?'
        }
      }
    }
  }
});

// Create indexes for declarationReadModel
db.declarationReadModel.createIndex({ declarationId: 1 }, { unique: true });
db.declarationReadModel.createIndex({ numeroContrat: 1 });
db.declarationReadModel.createIndex({ typeSinistre: 1 });
db.declarationReadModel.createIndex({ dateSinistre: 1 });
db.declarationReadModel.createIndex({ statut: 1 });
db.declarationReadModel.createIndex({ dateDeclaration: 1 });
db.declarationReadModel.createIndex({ numeroContrat: 1, dateSinistre: -1 });

// Create indexes for typeSinistreReference
db.typeSinistreReference.createIndex({ code: 1 }, { unique: true });
db.typeSinistreReference.createIndex({ actif: 1 });

// Insert reference data for claim types
db.typeSinistreReference.insertMany([
  {
    code: 'ACCIDENT_CORPOREL',
    libelle: 'Accident Corporel',
    description: 'Sinistre causant des blessures physiques ou des dommages corporels à une ou plusieurs personnes.',
    actif: true
  },
  {
    code: 'DEGATS_MATERIELS',
    libelle: 'Dégâts Matériels',
    description: 'Sinistre causant des dommages aux biens matériels (véhicules, bâtiments, équipements, etc.).',
    actif: true
  },
  {
    code: 'RESPONSABILITE_CIVILE',
    libelle: 'Responsabilité Civile',
    description: 'Sinistre engageant la responsabilité civile de l\'assuré envers un tiers.',
    actif: true
  }
]);

// Create application user with read/write permissions
db.createUser({
  user: 'cap_bsp_dsp_user',
  pwd: 'dsp_password_123',
  roles: [
    {
      role: 'readWrite',
      db: 'cap_bsp_dsp'
    }
  ]
});

print('MongoDB initialization completed successfully');
print('Database: cap_bsp_dsp');
print('Collections created: declarationReadModel, typeSinistreReference');
print('Reference data inserted: 3 claim types');
print('Application user created: cap_bsp_dsp_user');
