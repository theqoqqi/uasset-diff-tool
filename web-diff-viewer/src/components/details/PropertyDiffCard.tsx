import React from 'react';
import ExpandableCard from './ExpandableCard';
import type { PropertyDiff, ValueChange, FlagsChange } from '../../diffs/types';

interface PropertyDiffCardProps {
    name: string;
    data: PropertyDiff;
    prefix?: string;
}

const PropertyDiffCard: React.FC<PropertyDiffCardProps> = ({
    name,
    data,
    prefix = 'Property'
}) => {
    const renderField = <T extends string | number | null>(
        label: string,
        change: ValueChange<T> | FlagsChange<string> | undefined
    ) => {
        if (!change) return null;
        return <p>{label}: {change.From ?? ''} → {change.To ?? ''}</p>;
    };

    return (
        <ExpandableCard
            title={`${prefix}: ${name}`}
            status={data.DiffType}
        >
            {renderField('Type', data.Type)}
            {renderField('StructClass', data.StructClass)}
            {renderField('PropertyClass', data.PropertyClass)}
            {renderField('ArrayDim', data.ArrayDim)}
            {renderField('Flags', data.PropertyFlags)}
            {Object.entries(data.InnerProperties ?? {}).map(([innerName, innerData]) => (
                <PropertyDiffCard
                    key={innerName}
                    name={innerName}
                    data={innerData}
                    prefix="Inner property"
                />
            ))}
        </ExpandableCard>
    );
};

export default PropertyDiffCard;
