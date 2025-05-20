import React from 'react';
import ExpandableCard from './ExpandableCard';
import PropertyDiffCard from './PropertyDiffCard';
import type { FunctionDiff, PropertyDiff } from '../../diffs/types';

interface FunctionDiffCardProps {
    name: string;
    data: FunctionDiff;
}

const FunctionDiffCard: React.FC<FunctionDiffCardProps> = ({ name, data }) => {
    return (
        <ExpandableCard
            title={`Function: ${name}`}
            status={data.DiffType}
        >
            <p>Flags: {data.FunctionFlags?.From || ''} → {data.FunctionFlags?.To || ''}</p>
            {Object.entries(data.InputProperties ?? {}).map(([propName, propData]) => (
                <PropertyDiffCard
                    key={`in-${propName}`}
                    name={propName}
                    data={propData as PropertyDiff}
                    prefix="In"
                />
            ))}
            {Object.entries(data.OutputProperties ?? {}).map(([propName, propData]) => (
                <PropertyDiffCard
                    key={`out-${propName}`}
                    name={propName}
                    data={propData as PropertyDiff}
                    prefix="Out"
                />
            ))}
        </ExpandableCard>
    );
};

export default FunctionDiffCard;
