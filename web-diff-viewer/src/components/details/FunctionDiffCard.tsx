import React from 'react';
import ExpandableCard from './ExpandableCard';
import PropertyDiffCard from './PropertyDiffCard';
import type { FunctionDiff, PropertyDiff } from '../../diffs/types';

interface FunctionDiffCardProps {
    name: string;
    diff: FunctionDiff;
}

const FunctionDiffCard: React.FC<FunctionDiffCardProps> = ({name, diff}) => {
    return (
        <ExpandableCard
            title={`Function: ${name}`}
            status={diff.DiffType}
        >
            <p>Flags: {diff.FunctionFlags?.From || ''} → {diff.FunctionFlags?.To || ''}</p>
            {Object.entries(diff.InputProperties ?? {}).map(([propName, propData]) => (
                <PropertyDiffCard
                    key={`in-${propName}`}
                    name={propName}
                    diff={propData as PropertyDiff}
                    prefix="In"
                />
            ))}
            {Object.entries(diff.OutputProperties ?? {}).map(([propName, propData]) => (
                <PropertyDiffCard
                    key={`out-${propName}`}
                    name={propName}
                    diff={propData as PropertyDiff}
                    prefix="Out"
                />
            ))}
        </ExpandableCard>
    );
};

export default FunctionDiffCard;
