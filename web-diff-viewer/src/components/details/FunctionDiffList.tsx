import React from 'react';
import FunctionDiffCard from './FunctionDiffCard';
import type { FunctionDiff } from '../../diffs/types';

interface FunctionListProps {
    functions?: Record<string, FunctionDiff>;
}

const FunctionDiffList: React.FC<FunctionListProps> = ({ functions }) => {
    return (
        <div>
            {Object.entries(functions ?? {}).map(([name, diff]) => (
                <FunctionDiffCard key={name} name={name} diff={diff} />
            ))}
        </div>
    );
};

export default FunctionDiffList;
