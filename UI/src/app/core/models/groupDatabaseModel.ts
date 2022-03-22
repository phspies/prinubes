import { OrganizationDatabaseModel } from './organizationDatabaseModel';
import { UserDatabaseModel } from './userDatabaseModel';


export interface GroupDatabaseModel { 
    group?: string | null;
    organization_id?: string;
    organization?: OrganizationDatabaseModel;
    users?: Array<UserDatabaseModel> | null;
    id: string;
    create_timestamp?: string;
    update_timestamp?: string;
    row_version?: string | null;
}

