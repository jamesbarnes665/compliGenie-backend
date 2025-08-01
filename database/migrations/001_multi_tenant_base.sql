-- Base tenant table
CREATE TABLE tenants (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    subdomain VARCHAR(100) UNIQUE,
    api_key_hash VARCHAR(255) UNIQUE,
    stripe_account_id VARCHAR(255),
    settings JSONB DEFAULT '{}',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Tenant-scoped tables use composite keys
CREATE TABLE policies (
    id UUID DEFAULT gen_random_uuid(),
    tenant_id UUID REFERENCES tenants(id),
    client_name VARCHAR(255),
    content JSONB,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (tenant_id, id)
);

-- Payment tracking tables
CREATE TABLE webhook_events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    stripe_event_id VARCHAR(255) UNIQUE,
    type VARCHAR(100),
    processed BOOLEAN DEFAULT FALSE,
    tenant_id UUID REFERENCES tenants(id),
    payload JSONB,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE financial_alerts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    type VARCHAR(100),
    severity VARCHAR(20),
    message TEXT,
    resolved BOOLEAN DEFAULT FALSE,
    tenant_id UUID REFERENCES tenants(id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Row Level Security
ALTER TABLE policies ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation ON policies
    USING (tenant_id = current_setting('app.current_tenant')::uuid);

ALTER TABLE webhook_events ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_webhook_isolation ON webhook_events
    USING (tenant_id = current_setting('app.current_tenant')::uuid);
