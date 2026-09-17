import { useEffect, useState } from 'react';
import { Activity, Cpu, FileText, ShieldCheck, UploadCloud, Zap } from 'lucide-react';
import { getDesign, HardwareDesign, ReviewResult, runReview } from './api';
import UploadPage from './UploadPage';
import ReviewPage from './ReviewPage';

type RunState = 'upload' | 'running' | 'complete';

const agentSteps = [
  { name: 'Circuit Analysis Agent', detail: 'Analyzing circuit topology...', icon: Cpu },
  { name: 'Datasheet Agent', detail: 'Extracting component constraints...', icon: FileText },
  { name: 'Compliance Agent', detail: 'Checking engineering rules...', icon: ShieldCheck },
  { name: 'Reviewer Agent', detail: 'Verifying findings...', icon: Activity },
];

export default function App() {
  const [design, setDesign] = useState<HardwareDesign | null>(null);
  const [review, setReview] = useState<ReviewResult | null>(null);
  const [runState, setRunState] = useState<RunState>('upload');
  const [activeStep, setActiveStep] = useState(0);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getDesign().then(setDesign).catch((err: Error) => setError(err.message));
  }, []);

  async function handleRun() {
    setError(null);
    setReview(null);
    setRunState('running');
    setActiveStep(0);

    for (let step = 0; step < agentSteps.length; step += 1) {
      setActiveStep(step);
      await new Promise((resolve) => window.setTimeout(resolve, 650));
    }

    try {
      const result = await runReview();
      setReview(result);
      setRunState('complete');
    } catch (err) {
      setRunState('upload');
      setError(err instanceof Error ? err.message : 'Unable to run review.');
    }
  }

  return (
    <main className="min-h-screen bg-slate-950 text-slate-100">
      <div className="mx-auto flex min-h-screen w-full max-w-7xl flex-col px-5 py-6 md:px-8">
        <header className="flex flex-wrap items-center justify-between gap-4 border-b border-slate-800 pb-5">
          <div className="flex items-center gap-3">
            <div className="grid h-11 w-11 place-items-center rounded bg-cyan-400 text-slate-950">
              <Zap size={24} />
            </div>
            <div>
              <p className="text-xs font-semibold uppercase tracking-wide text-cyan-300">Agentic AI POC</p>
              <h1 className="text-xl font-semibold text-white md:text-2xl">Agentic Hardware Design Reviewer</h1>
            </div>
          </div>
          <div className="flex items-center gap-2 text-sm text-slate-300">
            <UploadCloud size={18} />
            Mock artifacts only
          </div>
        </header>

        {error && <div className="mt-5 border border-red-500/40 bg-red-950/50 px-4 py-3 text-red-100">{error}</div>}

        {runState === 'upload' && <UploadPage design={design} onRun={handleRun} />}
        {runState === 'running' && <AgentActivity activeStep={activeStep} />}
        {runState === 'complete' && review && design && <ReviewPage design={design} review={review} onRunAgain={handleRun} />}
      </div>
    </main>
  );
}

function AgentActivity({ activeStep }: { activeStep: number }) {
  return (
    <section className="grid flex-1 content-center gap-8 py-10">
      <div>
        <p className="text-sm font-semibold uppercase tracking-wide text-cyan-300">Agent Activity</p>
        <h2 className="mt-2 text-3xl font-semibold text-white">Running agentic review</h2>
      </div>
      <div className="grid gap-3">
        {agentSteps.map((step, index) => {
          const Icon = step.icon;
          const complete = index <= activeStep;
          return (
            <div key={step.name} className="flex items-center justify-between border border-slate-800 bg-slate-900 px-4 py-4">
              <div className="flex items-center gap-4">
                <div className={`grid h-10 w-10 place-items-center rounded ${complete ? 'bg-cyan-400 text-slate-950' : 'bg-slate-800 text-slate-400'}`}>
                  <Icon size={20} />
                </div>
                <div>
                  <h3 className="font-semibold text-white">{step.name}</h3>
                  <p className="text-sm text-slate-400">{step.detail}</p>
                </div>
              </div>
              <span className={complete ? 'text-cyan-300' : 'text-slate-500'}>{complete ? 'Complete' : 'Waiting'}</span>
            </div>
          );
        })}
      </div>
    </section>
  );
}
