import { AlertTriangle, RotateCcw } from 'lucide-react';
import { Finding, HardwareDesign, ReviewResult } from './api';

type Props = {
  design: HardwareDesign;
  review: ReviewResult;
  onRunAgain: () => void;
};

export default function ReviewPage({ design, review, onRunAgain }: Props) {
  const voltageFinding = review.findings.find((finding) => finding.title === 'Voltage Domain Violation');

  return (
    <section className="grid gap-7 py-7">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <p className="text-sm font-semibold uppercase tracking-wide text-cyan-300">{design.name}</p>
          <h2 className="mt-2 text-3xl font-semibold text-white">AGENTIC HARDWARE REVIEW</h2>
          <p className="mt-2 text-lg font-semibold text-red-300">{review.overallStatus.replaceAll('_', ' ')}</p>
        </div>
        <button className="flex items-center gap-2 border border-slate-700 px-4 py-2 text-sm font-semibold text-slate-200 hover:border-cyan-300 hover:text-cyan-200" onClick={onRunAgain}>
          <RotateCcw size={16} />
          Run Again
        </button>
      </div>

      <div className="grid gap-3 md:grid-cols-4 lg:grid-cols-8">
        <Score score={review.score} />
        <Metric label="Components" value={review.metrics.components} />
        <Metric label="Nets" value={review.metrics.nets} />
        <Metric label="Datasheets" value={review.metrics.datasheets} />
        <Metric label="Findings" value={review.metrics.findings} />
        <Metric label="Critical" value={review.metrics.critical} tone="critical" />
        <Metric label="High" value={review.metrics.high} tone="high" />
        <Metric label="Medium" value={review.metrics.medium} tone="medium" />
      </div>

      {voltageFinding && <Traceability finding={voltageFinding} />}

      <div className="grid gap-4">
        {review.findings.map((finding) => (
          <FindingCard key={`${finding.component}-${finding.title}`} finding={finding} />
        ))}
      </div>
    </section>
  );
}

function Score({ score }: { score: number }) {
  return (
    <div className="border border-cyan-400/40 bg-cyan-400/10 p-4 md:col-span-2">
      <div className="text-sm uppercase tracking-wide text-cyan-200">Compliance Score</div>
      <div className="mt-1 text-4xl font-semibold text-white">{score}%</div>
    </div>
  );
}

function Metric({ label, value, tone }: { label: string; value: number; tone?: 'critical' | 'high' | 'medium' }) {
  const toneClass = tone === 'critical' ? 'text-red-300' : tone === 'high' ? 'text-orange-300' : tone === 'medium' ? 'text-yellow-200' : 'text-white';
  return (
    <div className="border border-slate-800 bg-slate-900 p-4">
      <div className={`text-3xl font-semibold ${toneClass}`}>{value}</div>
      <div className="text-xs uppercase tracking-wide text-slate-400">{label}</div>
    </div>
  );
}

function Traceability({ finding }: { finding: Finding }) {
  return (
    <div className="border border-slate-800 bg-slate-900 p-5">
      <div className="flex items-center gap-3">
        <AlertTriangle className="text-red-300" size={20} />
        <div>
          <h3 className="font-semibold text-white">Why was this flagged?</h3>
          <p className="text-sm text-slate-400">Traceability for Voltage Domain Violation</p>
        </div>
      </div>
      <div className="mt-5 grid gap-3 md:grid-cols-5">
        {finding.traceability.map((step, index) => (
          <div key={`${step.stage}-${step.detail}`} className="relative border border-slate-700 bg-slate-950 p-4">
            <div className="text-xs uppercase tracking-wide text-cyan-300">{step.stage}</div>
            <div className="mt-2 min-h-10 text-sm font-semibold text-white">{step.detail}</div>
            {index < finding.traceability.length - 1 && <div className="absolute -right-3 top-1/2 hidden h-px w-6 bg-cyan-400 md:block" />}
          </div>
        ))}
      </div>
    </div>
  );
}

function FindingCard({ finding }: { finding: Finding }) {
  const severityClass = finding.severity === 'CRITICAL' ? 'bg-red-500 text-white' : finding.severity === 'HIGH' ? 'bg-orange-400 text-slate-950' : 'bg-yellow-300 text-slate-950';

  return (
    <article className="border border-slate-800 bg-slate-900 p-5">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <span className={`inline-block px-3 py-1 text-xs font-bold ${severityClass}`}>{finding.severity}</span>
          <h3 className="mt-3 text-xl font-semibold text-white">{finding.title}</h3>
          <p className="text-slate-400">{finding.component} - {finding.issue}</p>
        </div>
        <div className="text-right text-sm text-slate-400">Rule <span className="font-semibold text-white">{finding.rule}</span></div>
      </div>

      <div className="mt-5 grid gap-3 md:grid-cols-4">
        <Fact label="Expected" value={finding.expected} />
        <Fact label="Detected" value={finding.detected} />
        <Fact label="Evidence" value={finding.evidence} />
        <Fact label="Rule" value={finding.rule} />
      </div>

      <div className="mt-5 grid gap-4 md:grid-cols-2">
        <div>
          <h4 className="text-xs font-semibold uppercase tracking-wide text-cyan-300">AI Reasoning</h4>
          <p className="mt-2 text-sm leading-6 text-slate-300">{finding.reason}</p>
        </div>
        <div>
          <h4 className="text-xs font-semibold uppercase tracking-wide text-cyan-300">Recommendation</h4>
          <p className="mt-2 text-sm leading-6 text-slate-300">{finding.recommendation}</p>
        </div>
      </div>
    </article>
  );
}

function Fact({ label, value }: { label: string; value: string }) {
  return (
    <div className="border border-slate-700 bg-slate-950 p-3">
      <div className="text-xs font-semibold uppercase tracking-wide text-slate-500">{label}</div>
      <div className="mt-1 break-words text-sm font-semibold text-slate-100">{value}</div>
    </div>
  );
}
