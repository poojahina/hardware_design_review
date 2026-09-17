export type HardwareDesign = {
  name: string;
  purpose: string;
  components: Component[];
  netlist: NetConnection[];
  datasheets: Datasheet[];
  engineeringRules: EngineeringRule[];
};

export type Component = {
  reference: string;
  name: string;
  type: string;
  value: string | null;
};

export type NetConnection = {
  from: string;
  to: string;
};

export type Datasheet = {
  component: string;
  constraints: DatasheetConstraint[];
};

export type DatasheetConstraint = {
  type: string;
  min: number | null;
  max: number | null;
  required: string | null;
  approvedValue: string | null;
};

export type EngineeringRule = {
  code: string;
  description: string;
};

export type TraceStep = {
  stage: string;
  detail: string;
};

export type Finding = {
  component: string;
  title: string;
  issue: string;
  severity: 'CRITICAL' | 'HIGH' | 'MEDIUM' | string;
  expected: string;
  detected: string;
  evidence: string;
  rule: string;
  reason: string;
  recommendation: string;
  traceability: TraceStep[];
};

export type ReviewResult = {
  overallStatus: string;
  score: number;
  summary: string;
  metrics: {
    components: number;
    nets: number;
    datasheets: number;
    findings: number;
    critical: number;
    high: number;
    medium: number;
  };
  findings: Finding[];
};

const apiBase = import.meta.env.VITE_API_BASE_URL ?? '';

export async function getDesign(): Promise<HardwareDesign> {
  try {
    const response = await fetch(`${apiBase}/api/review/design`);
    if (!response.ok) {
      throw new Error('Unable to load mock design.');
    }

    return response.json();
  } catch {
    const { mockDesign } = await import('./mockData');
    return mockDesign;
  }
}

export async function runReview(): Promise<ReviewResult> {
  try {
    const response = await fetch(`${apiBase}/api/review/analyze`, {
      method: 'POST',
    });

    if (!response.ok) {
      throw new Error('Unable to run hardware review.');
    }

    return response.json();
  } catch {
    const { mockReviewResult } = await import('./mockData');
    return mockReviewResult;
  }
}
