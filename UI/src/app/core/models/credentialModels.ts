import { OrganizationDatabaseModel } from './organizationDatabaseModel';


export interface CredentialDatabaseModel { 
    credential?: string | null;
    organization_id?: string;
    username: string;
    organization?: OrganizationDatabaseModel;
    id: string;
    create_timestamp?: string;
    update_timestamp?: string;
    row_version?: string | null;
}

export interface CredentialCRUDModel { 
    credential?: string;
    username: string;
    password: string;
    row_version?: string;
    
}

