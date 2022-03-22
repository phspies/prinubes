import { TaggingDatabaseModel } from './taggingDatabaseModel';
import { PlatformType } from './platformType';
import { OrganizationDatabaseModel } from './organizationDatabaseModel';


export interface ComputePlatformCRUDModel { 
    platform?: string | null;
    organization_id?: string | null;
    platform_type?: PlatformType;
    credential_id?: string;
    location?: string | null;
    url_endpoint?: string | null;
    verify_ssl_cert?: boolean;
    tags?: Array<TaggingDatabaseModel> | null;
    datacenter_moid?: string | null;
    clusters_moid?: string | null;
    folder_moid?: string | null;
    resourcepool_moid?: string | null;
    row_version?: string | null;
}

export interface ComputePlatformDatabaseModel { 
    id?: string | null;
    platform?: string | null;
    organization_id?: string | null;
    platform_type?: PlatformType;
    credential_id?: string;
    location?: string | null;
    url_endpoint?: string | null;
    verify_ssl_cert?: boolean;
    tags?: Array<TaggingDatabaseModel> | null;
    datacenter_moid?: string | null;
    clusters_moid?: string | null;
    folder_moid?: string | null;
    resourcepool_moid?: string | null;
    create_timestamp?: string;
    update_timestamp?: string;
    row_version?: string | null;
}

